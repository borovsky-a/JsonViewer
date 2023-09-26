using System.Windows;

namespace mop.Configurator
{
    public static class ApplicationExtensions
    {
        private static MainWindow window;
        private static MainViewModel viewModel;
        public static ConfigurationProvider AddConfiguratorUI(this Application application)
        {
            window = new MainWindow();
            application.MainWindow = window;
            viewModel = new MainViewModel();
            window.DataContext = viewModel;
            return viewModel.ConfigurationProvider;
        }

        //public static ConfigurationProvider ClearDefaultsManagers(this ConfigurationProvider configurationProvider)
        //{
        //    configurationProvider.RegisteredConfigurationManagers.Clear();
        //    return configurationProvider;
        //}
        //public static ConfigurationProvider RegisterConfigurationManager(this ConfigurationProvider configurationProvider, IConfigurationManager configurationManager)
        //{
        //    configurationManager.ConfigurationProvider = configurationProvider;
        //    configurationProvider.RegisteredConfigurationManagers.Add(configurationManager);
        //    return configurationProvider;
        //}

        public static void Show(this ConfigurationProvider configurationProvider, bool refresh = false)
        {
            //if (refresh)
            //    configurationProvider.Refresh();
            window.Show();
        }
    }
}
