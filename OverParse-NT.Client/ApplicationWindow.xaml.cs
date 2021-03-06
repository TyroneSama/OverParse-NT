﻿using OverParse_NT.Client.Controls;
using OverParse_NT.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.IO;
using System.Windows.Threading;
using OverParse_NT.PSRT;
using System.Runtime.ExceptionServices;
using OverParse_NT.DamageDump;
using System.Diagnostics;

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for ApplicationWindow.xaml
    /// </summary>
    public partial class ApplicationWindow : Window
    {
        private CancellationTokenSource _BackgroundRunnerTokenSource = new CancellationTokenSource();
        private Task _BackgroundRunnerTask;
        private SemaphoreSlim _BackgroundSemaphore = new SemaphoreSlim(1);

        private enum DamageDisplayListState
        {
            Waiting,
            Loading,
            Visible
        }

        // TODO: replace this with actual bindings
        private DamageDisplayListState _DamageDispalyListState
        {
            set
            {
                _DamageDisplayList.Visibility = value == DamageDisplayListState.Visible ? Visibility.Visible : Visibility.Collapsed;
                _DamageDisplayListLoading.Visibility = value == DamageDisplayListState.Loading ? Visibility.Visible : Visibility.Collapsed;
                _DamageDisplayListWaiting.Visibility = value == DamageDisplayListState.Waiting ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string _DamageDumpFolder => Path.Combine(Properties.Settings.Default.PSO2BinDirectory, "damagelogs");

        public ApplicationWindow()
        {
            InitializeComponent();

            _DamageDispalyListState = DamageDisplayListState.Waiting;

            _BackgroundRunnerTask = _BackgroundRunner(_BackgroundRunnerTokenSource.Token).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                    ExceptionDispatchInfo.Capture(t.Exception.InnerException).Throw();
            });
        }

        private DamageDumpCharacterAccumulator _Accumulator = new DamageDumpCharacterAccumulator();

        private async Task _BackgroundRunner(CancellationToken ct)
        {
            _Accumulator.InfosChanged += (sender, infos) =>
            {
                var ordered = infos.OrderByDescending(x => x.TotalDamage);
                var items = ordered.Select(i => new DamageDisplayList.DamageDisplayData
                {
                    Name = i.Name,
                    Damage = i.TotalDamage,
                    DamageRatio = 100.0 * i.TotalDamage / ordered.Sum(x => x.TotalDamage),
                    DamageRatioRelative = (100.0 / ordered.First().TotalDamage) * i.TotalDamage,
                    MaxHitDamage = i.MaxHitDamage,
                    MaxHitName = $"Id: {i.MaxHitAction}"
                });

                Dispatcher.Invoke(() =>
                {
                    _DamageDisplayList.Items.Clear();
                    foreach (var i in items)
                        _DamageDisplayList.Items.Add(i);
                    _DamageDispalyListState = DamageDisplayListState.Visible;
                });
            };

            // TODO: this should probably be done in a better way.
            // at the moment if anything goes wrong the system will
            // reset the accumulator and start again, ~better than a crash?~
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // wait for file to show up
                var logFile = _GetLatestLogFile(_DamageDumpFolder);
                if (logFile == null)
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                try
                {
                    Dispatcher.Invoke(() => _DamageDispalyListState = DamageDisplayListState.Loading);

                    var watcher = new LogFileWatcher();
                    watcher.OnNewEntries += async (sender, entries) =>
                    {
                        await _BackgroundSemaphore.WaitAsync();
                        try
                        {
                            _Accumulator.ProcessEntries(entries);
                        }
                        finally
                        {
                            _BackgroundSemaphore.Release();
                        }
                    };

                    using (var fsWatcher = new FileSystemWatcher())
                    {
                        fsWatcher.Path = _DamageDumpFolder;

                        var fsWatcherSource = new CancellationTokenSource();
                        void OnFsChanged(object source, object _)
                        {
                            if (_GetLatestLogFile(_DamageDumpFolder) != logFile)
                                fsWatcherSource.Cancel();
                        }

                        fsWatcher.Created += OnFsChanged;
                        fsWatcher.Deleted += OnFsChanged;
                        fsWatcher.Changed += OnFsChanged;
                        fsWatcher.Renamed += OnFsChanged;

                        fsWatcher.EnableRaisingEvents = true;

                        await watcher.RunAsync(logFile, CancellationTokenSource.CreateLinkedTokenSource(ct, fsWatcherSource.Token).Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // this exception does not mean anything as there are multiple
                    // cancellation tokens that could have caused it, the function however
                    // only cancels to one specific token and that is handled separately
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in log file watcher loop -> {ex.Message}");
                }
                finally
                {
                    await _ResetAccumulator();
                    Dispatcher.Invoke(() => _DamageDispalyListState = DamageDisplayListState.Waiting);
                }
            }
        }

        private async Task _ResetAccumulator()
        {
            await _BackgroundSemaphore.WaitAsync();
            try
            {
                // TODO: this should be some form of encounter end
                // instead of just discarding the data
                _Accumulator.Reset();
            }
            finally
            {
                _BackgroundSemaphore.Release();
            }
        }

        private string _GetLatestLogFile(string logsDirectory)
        {
            if (!Directory.Exists(logsDirectory))
                return null;

            var logs = Directory.GetFiles(logsDirectory, "*.csv").ToList();
            if (logs.Count == 0)
                return null;

            logs.Sort(Comparer<string>.Create((x, y) =>
            {
                var resX = 0L;
                if (!long.TryParse(Path.GetFileNameWithoutExtension(x), out resX))
                    return -1;
                var resY = 0L;
                if (!long.TryParse(Path.GetFileNameWithoutExtension(y), out resY))
                    return 1;
                return resX.CompareTo(resY);
            }));
            logs.Reverse();

            return logs.First();
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _BackgroundRunnerTokenSource.Cancel();
            if (_BackgroundRunnerTask != null)
                await _BackgroundRunnerTask;
        }

        private async void _ResetAccumulatorButtonPress(object sender, RoutedEventArgs e)
        {
            await _ResetAccumulator();
        }
    }
}
