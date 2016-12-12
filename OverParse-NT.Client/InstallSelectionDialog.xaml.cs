using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for InstallSelectionDialog.xaml
    /// </summary>
    public partial class InstallSelectionDialog : Window
    {
        public InstallSelectionDialog()
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(Properties.Settings.Default.PSO2BinDirectory))
            {
                _SwitchToPluginInstallDialog();
                return;
            }

            _PathInputField_TextChanged(null, null); // trigger event handler to reset state
        }

        private void _SwitchToPluginInstallDialog()
        {
            var dialog = new PluginInstallDialog();
            System.Windows.Application.Current.MainWindow = dialog;
            if (!dialog.IsClosed)
                dialog.Show();
            Close();
        }

        private void _PathInputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: enum declaring validation state so that the feedback can react

            _ConfirmButton.IsEnabled = false; // confirm button only enabled if all tests are passed
            var text = _PathInputField.Text;

            if (text.Length == 0)
            {
                _PathValidationFeedback.Content = "Please select a directory";
                _PathValidationFeedback.Foreground = Brushes.Orange;
                return;
            }

            if (!Directory.Exists(text))
            {
                _PathValidationFeedback.Content = "Path is not a valid directory";
                _PathValidationFeedback.Foreground = Brushes.Red;
                return;
            }

            if (new DirectoryInfo(text).Name != "pso2_bin")
            {
                _PathValidationFeedback.Content = "Directory must be named `pso2__bin`"; // double underscore cus windows
                _PathValidationFeedback.Foreground = Brushes.Red;
                return;
            }

            if (Directory.GetFiles(text, "pso2.exe").Length == 0)
            {
                _PathValidationFeedback.Content = "Directory does not contain `pso2.exe` executable";
                _PathValidationFeedback.Foreground = Brushes.Red;
                return;
            }

            _PathValidationFeedback.Content = "Valid `pso2__bin` directory selected!";
            _PathValidationFeedback.Foreground = Brushes.Green;
            _ConfirmButton.IsEnabled = true;
        }

        private void _ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PSO2BinDirectory = _PathInputField.Text;
            Properties.Settings.Default.Save();

            _SwitchToPluginInstallDialog();
        }

        private void _PathInputBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = "Please select your `pso2_bin` install directory"
            };

            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _PathInputField.Text = browserDialog.SelectedPath;
        }
    }
}
