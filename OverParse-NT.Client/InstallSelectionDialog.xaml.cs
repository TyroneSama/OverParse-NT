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
                _SwitchToApplicationWindow();
                return;
            }

            _PathInputField_TextChanged(null, null); // trigger event handler to reset state
        }

        private void _SwitchToApplicationWindow()
        {
            var window = new ApplicationWindow();
            System.Windows.Application.Current.MainWindow = window;
            window.Show();
            Close();
        }

        private void _PathInputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: enum declaring validation state so that the feedback can react

            _ConfirmButton.IsEnabled = false; // confirm button only enabled if all tests are passed
            var text = _PathInputField.Text;

            if (text.Length == 0)
            {
                BindingOperations.ClearBinding(_PathValidationFeedback, ContentProperty);
                BindingOperations.SetBinding(_PathValidationFeedback, ContentProperty, new LocaleExtension("InstallSelectionDialog_ValidationNoDirectory"));
                _PathValidationFeedback.Foreground = Brushes.Orange;
                return;
            }

            if (!Directory.Exists(text))
            {
                BindingOperations.ClearBinding(_PathValidationFeedback, ContentProperty);
                BindingOperations.SetBinding(_PathValidationFeedback, ContentProperty, new LocaleExtension("InstallSelectionDialog_ValidationInvalidDirectory"));
                _PathValidationFeedback.Foreground = Brushes.Red;
                return;
            }

            if (new DirectoryInfo(text).Name != "pso2_bin")
            {
                BindingOperations.ClearBinding(_PathValidationFeedback, ContentProperty);
                BindingOperations.SetBinding(_PathValidationFeedback, ContentProperty, new LocaleExtension("InstallSelectionDialog_ValidationWrongDirectoryName"));
                _PathValidationFeedback.Foreground = Brushes.Red;
                return;
            }

            if (Directory.GetFiles(text, "pso2.exe").Length == 0)
            {
                BindingOperations.ClearBinding(_PathValidationFeedback, ContentProperty);
                BindingOperations.SetBinding(_PathValidationFeedback, ContentProperty, new LocaleExtension("InstallSelectionDialog_ValidationNoExecutable"));
                _PathValidationFeedback.Foreground = Brushes.Red;
                return;
            }

            BindingOperations.ClearBinding(_PathValidationFeedback, ContentProperty);
            BindingOperations.SetBinding(_PathValidationFeedback, ContentProperty, new LocaleExtension("InstallSelectionDialog_ValidationValid"));
            _PathValidationFeedback.Foreground = Brushes.Green;
            _ConfirmButton.IsEnabled = true;
        }

        private void _ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.PSO2BinDirectory = _PathInputField.Text;
            Properties.Settings.Default.Save();

            _SwitchToApplicationWindow();
        }

        private void _PathInputBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var browserDialog = new FolderBrowserDialog()
            {
                ShowNewFolderButton = false,
                Description = LocaleManager.Instance["InstallSelectionDialog_BrowserDescription"]
            };

            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _PathInputField.Text = browserDialog.SelectedPath;
        }
    }
}
