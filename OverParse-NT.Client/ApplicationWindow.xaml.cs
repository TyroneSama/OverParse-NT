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

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for ApplicationWindow.xaml
    /// </summary>
    public partial class ApplicationWindow : Window
    {
        private DamageDumpManager _Manager;
        private CancellationTokenSource _ManagerTokenSource = new CancellationTokenSource();
        private Task _ManagerTask;

        public ApplicationWindow(string pso2InstallLocation)
        {
            InitializeComponent();

            // temp
            var logDirectoryPath = Path.Combine(pso2InstallLocation, "damagelogs");
            if (!Directory.Exists(logDirectoryPath))
                throw new Exception("Logs directory does not exist"); // TODO

            var logFiles = Directory.GetFiles(logDirectoryPath, "*.csv").ToList();
            if (logFiles.Count == 0)
                throw new Exception("No log files found");

            logFiles.Sort();
            logFiles.Reverse();
            //

            _Manager = new DamageDumpManager(logFiles.First());
            _Manager.DataChanged += (sender, args) => _UpdateDisplayList(args);

            _ManagerTask = _Manager.Run(_ManagerTokenSource.Token);
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

        protected async override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _ManagerTokenSource.Cancel();
            if (_ManagerTask != null)
                await _ManagerTask.ContinueWith((_) => { });
        }
    }
}
