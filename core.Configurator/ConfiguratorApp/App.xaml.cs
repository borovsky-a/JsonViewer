using mop.Configurator;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ConfiguratorApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            this.AddConfiguratorUI()
                //.RegisterConfigurationManager(new FileXmlConfigurationManager(@"..\file\\cnt.HttpsReceiver.configuration.xml"))
                .Show(true);
            base.OnStartup(e);
        }
    }
}
