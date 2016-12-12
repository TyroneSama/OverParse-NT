using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace OverParse_NT.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Error logging hack
            Action<Exception> handleException = ex =>
            {
                MessageBox.Show(e.ToString(), "Application closed due to unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            };
            Dispatcher.UnhandledException += (_, ex) => handleException(ex.Exception);
            AppDomain.CurrentDomain.UnhandledException += (_, ex) => handleException(ex.ExceptionObject as Exception);

            base.OnStartup(e);
        }
    }
}
