using OverParse_NT.Client.Controls;
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

        private string _DamageDumpFolder => Path.Combine(Properties.Settings.Default.PSO2BinDirectory, @"damagelogs");

        public ApplicationWindow()
        {
            InitializeComponent();

            _BackgroundRunnerTask = _BackgroundRunner(_BackgroundRunnerTokenSource.Token).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                    ExceptionDispatchInfo.Capture(t.Exception.InnerException).Throw();
            });
        }

        private async Task _BackgroundRunner(CancellationToken ct)
        {
            var accumulator = new DamageDumpCharacterAccumulator();
            accumulator.InfosChanged += (sender, infos) =>
            {
                var ordered = infos.OrderByDescending(x => x.TotalDamage);
                Dispatcher.Invoke(() =>
                {
                    _DamageDisplayList.Items.Clear();
                    foreach (var i in ordered)
                        _DamageDisplayList.Items.Add(new DamageDisplayList.DamageDisplayData
                        {
                            Name = i.Name,
                            Damage = i.TotalDamage,
                            DamageRatio = (100.0 / ordered.First().TotalDamage) * i.TotalDamage,
                            DamageRatioNormal = (100.0 / ordered.First().TotalDamage) * i.TotalDamage,
                            MaxHitDamage = i.MaxHitDamage,
                            MaxHitName = $"Id: {i.MaxHitAction}"
                        });
                });
            };

            // TODO: this should probably be done in a better way.
            // at the moment if anything goes wrong the system will
            // reset the accumulator and start again, ~better than a crash?~
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                // TODO: this along with the file spinner need to be event based
                // instead of spinner based
                var logFile = _GetLatestLogFile(_DamageDumpFolder);
                if (logFile == null)
                {
                    await Task.Delay(1000, ct);
                    continue;
                }

                try
                {
                    var watcher = new LogFileWatcher();
                    watcher.OnNewEntries += (sender, entries) => accumulator.ProcessEntries(entries);

                    var spinnerSource = new CancellationTokenSource();
                    // TODO
                    var fileSpinner = Task.Run(async () =>
                    {
                        while (true)
                        {
                            ct.ThrowIfCancellationRequested();

                            if (_GetLatestLogFile(_DamageDumpFolder) != logFile)
                            {
                                spinnerSource.Cancel();
                                break;
                            }
                            else
                            {
                                await Task.Delay(1000, ct);
                            }
                        }
                    });

                    await watcher.RunAsync(logFile, CancellationTokenSource.CreateLinkedTokenSource(ct, spinnerSource.Token).Token);
                    await fileSpinner;
                }
                catch (OperationCanceledException)
                {

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in log file watcher loop -> {ex.Message}");
                }
                finally
                {
                    // TODO: this should be some form of encounter end
                    // instead of just discarding the data
                    accumulator.Reset();
                }
            }
        }

        private string _GetLatestLogFile(string logsDirectory)
        {
            if (!Directory.Exists(logsDirectory))
                return null;

            var logs = Directory.GetFiles(logsDirectory, "*.csv").ToList();
            if (logs.Count == 0)
                return null;

            logs.Sort();
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
    }
}
