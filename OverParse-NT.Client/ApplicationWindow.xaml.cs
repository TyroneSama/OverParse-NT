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

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for ApplicationWindow.xaml
    /// </summary>
    public partial class ApplicationWindow : Window
    {
        private CancellationTokenSource _BackgroundRunnerTokenSource = new CancellationTokenSource();
        private Task _BackgroundRunnerTask;

        public ApplicationWindow()
        {
            InitializeComponent();

            // Error logging hack
            void handleException(Exception e)
            {
                MessageBox.Show(e.ToString(), "Application closed due to exception");
                Application.Current.Shutdown();
            };
            Dispatcher.UnhandledException += (_, e) => handleException(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, e) => handleException(e.ExceptionObject as Exception);
        }

        private void _UpdateDisplayList(DamageDumpManager.DataChangedEventArgs args)
        {
            var ordered = args.Entities.OrderByDescending(x => x.TotalDamage);
            Dispatcher.Invoke(() =>
            {
                _DamageDisplayList.Items.Clear();
                foreach (var e in ordered)
                    _DamageDisplayList.Items.Add(new DamageDisplayList.DamageDisplayData
                    {
                        Name = e.Name,
                        Damage = e.TotalDamage,
                        DamageRatio = (100.0 / ordered.First().TotalDamage) * e.TotalDamage,
                        DamageRatioNormal = (100.0 / ordered.First().TotalDamage) * (e.TotalDamage - e.ZanverseDamage),
                        DamageRatioZanverse = (100.0 / ordered.First().TotalDamage) * e.ZanverseDamage,
                        MaxHitDamage = e.MaxHitDamage,
                        MaxHitName = e.MaxHitName
                    });
            });
        }

        private async Task _BackgroundRunner(CancellationToken ct, string installLocation)
        {
            // temp
            var logDirectoryPath = Path.Combine(installLocation, "damagelogs");
            if (!Directory.Exists(logDirectoryPath))
                throw new Exception("Logs directory does not exist"); // TODO

            var logFiles = Directory.GetFiles(logDirectoryPath, "*.csv").ToList();
            if (logFiles.Count == 0)
                throw new Exception("No log files found");

            logFiles.Sort();
            logFiles.Reverse();
            //

            var manager = new DamageDumpManager(logFiles.First());
            manager.DataChanged += (sender, args) => _UpdateDisplayList(args);

            await manager.Run(ct);
        }

        protected async override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _BackgroundRunnerTokenSource.Cancel();
            if (_BackgroundRunnerTask != null)
                await _BackgroundRunnerTask;
        }

        private void _InstallSelectButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: non WinForms solution
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select your `pso2_bin` folder, this will be in the location you installed PSO2";
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                throw new Exception("DialogResult was not OK");

            bool isValidInstallPath(string path)
            {
                if (!Directory.Exists(path))
                    return false;

                if (Directory.GetFiles(path, "pso2.exe").Length == 0)
                    return false;

                return true; // more checks I guess
            }

            if (!isValidInstallPath(dialog.SelectedPath))
                throw new Exception("Path selected was not valid PSO2 install");

            _BackgroundRunnerTask = _BackgroundRunner(_BackgroundRunnerTokenSource.Token, dialog.SelectedPath);
            _InstallSelectWrapper.Visibility = Visibility.Collapsed;
        }
    }
}
