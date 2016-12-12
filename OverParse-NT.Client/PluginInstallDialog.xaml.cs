using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for PluginInstallDialog.xaml
    /// </summary>
    public partial class PluginInstallDialog : Window
    {
        public bool IsClosed { get; private set; }

        private bool _Install_ddraw;
        private bool _Install_pso2h;
        private bool _Install_PSO2DamageDump;

        private byte[] _Buffer_ddraw;
        private byte[] _Buffer_pso2h;
        private byte[] _Buffer_PSO2DamageDump;

        private string _Path_ddraw;
        private string _Path_pso2h;
        private string _Path_PSO2DamageDump;

        private CancellationTokenSource _TokenSource;
        private Task _BackgroundTask;

        public PluginInstallDialog()
        {
            InitializeComponent();
            IsClosed = false;

            _Buffer_ddraw = Properties.Resources.ResourceManager.GetObject("ddraw") as byte[];
            _Buffer_pso2h = Properties.Resources.ResourceManager.GetObject("pso2h") as byte[];
            _Buffer_PSO2DamageDump = Properties.Resources.ResourceManager.GetObject("PSO2DamageDump") as byte[];

            _Path_ddraw = Path.Combine(Properties.Settings.Default.PSO2BinDirectory, "ddraw.dll");
            _Path_pso2h = Path.Combine(Properties.Settings.Default.PSO2BinDirectory, "pso2h.dll");
            _Path_PSO2DamageDump = Path.Combine(Properties.Settings.Default.PSO2BinDirectory, @"plugins\PSO2DamageDump.dll");

            _Install_ddraw = _CompareInstalled(_Path_ddraw, _Buffer_ddraw);
            _Install_pso2h = _CompareInstalled(_Path_pso2h, _Buffer_pso2h);
            _Install_PSO2DamageDump = _CompareInstalled(_Path_PSO2DamageDump, _Buffer_PSO2DamageDump);

            if (!_Install_ddraw && !_Install_pso2h && !_Install_PSO2DamageDump)
            {
                // all plugins are updated
                _SwitchToApplicationWindow();
                return;
            }

            _TokenSource = new CancellationTokenSource();
            _BackgroundTask = Task.Run(async () =>
            {
                while(true)
                {
                    _TokenSource.Token.ThrowIfCancellationRequested();
                    Dispatcher.Invoke(() => { _InstallButton.IsEnabled = !Process.GetProcessesByName("pso2").Any(); });
                    await Task.Delay(TimeSpan.FromSeconds(2.5), _TokenSource.Token);
                }

            }).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                    ExceptionDispatchInfo.Capture(t.Exception.InnerException).Throw();
            });
        }

        private void _SwitchToApplicationWindow()
        {
            if (_TokenSource != null)
                _TokenSource.Cancel();
            if (_BackgroundTask != null)
                _BackgroundTask.Wait();

            var appWindow = new ApplicationWindow();
            Application.Current.MainWindow = appWindow;
            appWindow.Show();
            Close();
        }
        private bool _CompareInstalled(string installPath, byte[] resourceBuffer)
        {
            if (!File.Exists(installPath))
                return true;
            if (_GenerateHash(File.ReadAllBytes(installPath)) != _GenerateHash(resourceBuffer))
                return true;
            return false;
        }

        private string _GenerateHash(byte[] buffer)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(buffer);
                return string.Join(string.Empty, hash.Select(x => x.ToString("X2")));
            }
        }

        private void _SkipButton_Click(object sender, RoutedEventArgs e) => _SwitchToApplicationWindow();

        private void _InstallButton_Click(object sender, RoutedEventArgs e)
        {
            if (_Install_ddraw)
                File.WriteAllBytes(_Path_ddraw, _Buffer_ddraw);
            if (_Install_pso2h)
                File.WriteAllBytes(_Path_pso2h, _Buffer_pso2h);
            if (_Install_PSO2DamageDump)
                File.WriteAllBytes(_Path_PSO2DamageDump, _Buffer_PSO2DamageDump);

            _SwitchToApplicationWindow();
        }

        private void Window_Closed(object sender, EventArgs e) => IsClosed = true;
    }
}
