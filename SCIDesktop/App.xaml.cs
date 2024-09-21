using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SCICore.dao;
using SCICore.util;
using SCIDesktop.window;

namespace SCIDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // TODO: use e.Args to parse commandline args, mainly for --config
            // why using a separate thread? because async functions seems to block the UI thread --- told by GPT.
            var configDao = Task.Run(LoadConfig).Result;
            var mainWindow = new MainWindow(configDao);
            mainWindow.Show();
        }

        // see ParseGlobalOptions
        private static ConfigDao LoadConfig()
        {
            var configFilePath = ConfigUtils.FindConfigPath()!;

            if (configFilePath != null!)
            {
                return new ConfigDao(configFilePath);
            }

            var result = ConfigUtils.CreateDefaultConfig().Result;
            if (!result)
            {
                throw new Exception("no default config file. Creation failed as well.");
            }

            return new ConfigDao(ConfigUtils.GetDefaultConfigPath());
        }

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exceptionWindow = new ExceptionWindow(e.Exception);
            exceptionWindow.ShowDialog();
            // shutdown the program?
            e.Handled = true;
        }
    }
}