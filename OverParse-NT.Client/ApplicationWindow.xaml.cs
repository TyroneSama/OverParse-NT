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

            _BackgroundRunnerTask = _BackgroundRunner(_BackgroundRunnerTokenSource.Token).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                    ExceptionDispatchInfo.Capture(t.Exception.InnerException).Throw();
            });
        }

        private void _UpdateDisplayList(EncounterDisplayInfo info)
        {
            var ordered = info.Entities.OrderByDescending(x => x.TotalDamage);
            Dispatcher.Invoke(() =>
            {
                _DamageDisplayList.Items.Clear();
                foreach (var e in ordered)
                    _DamageDisplayList.Items.Add(new DamageDisplayList.DamageDisplayData
                    {
                        Name = e.Name,
                        Damage = e.TotalDamage,
                        DamageRatio = (100.0 / ordered.First().TotalDamage) * e.TotalDamage,
                        DamageRatioNormal = (100.0 / ordered.First().TotalDamage) * e.TotalDamage,
                        MaxHitDamage = e.StrongestAttack.Value,
                        MaxHitName = $"({e.StrongestAttack.Account}) {e.StrongestAttack.Name}"
                    });
            });
        }

        private async Task _BackgroundRunner(CancellationToken ct)
        {
            var generator = new PSRTGenerator();
            var manager = new GeneratorManager(generator);
            manager.EncounterInfoChanged += (sender, info) => _UpdateDisplayList(info);

            await generator.RunAsync(ct);
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
