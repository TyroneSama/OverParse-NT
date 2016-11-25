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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for InstallLocationDialog.xaml
    /// </summary>
    public partial class InstallLocationDialog : Window
    {
        public InstallLocationDialog()
        {
            InitializeComponent();
            SelectButton.IsEnabled = false;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var applicationWindow = new ApplicationWindow(SelectField.Text);
            applicationWindow.Show();
            Close();
        }

        private void SelectField_TextChanged(object sender, TextChangedEventArgs e)
        {
            SelectButton.IsEnabled = _IsValidInstallPath(SelectField.Text);
        }

        private bool _IsValidInstallPath(string path)
        {
            if (!Directory.Exists(path))
                return false;

            if (Directory.GetFiles(path, "pso2.exe").Length == 0)
                return false;

            return true; // more checks I guess
        }
    }
}
