
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace mop.Configurator
{
   public  class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            ConfigurationProvider = new ConfigurationProvider();
            ParametersViewModel = new ParametersViewModel(this);
            LogViewModel = new LogViewModel(this);
            ActionViewModel = new ActionViewModel(this);
            ProgramsViewModel = new ProgramsViewModel();
        }

        public ConfigurationProvider ConfigurationProvider { get; }

        public ParametersViewModel ParametersViewModel { get; }

        public LogViewModel LogViewModel { get; }

        public ActionViewModel ActionViewModel { get; }

        public ProgramsViewModel ProgramsViewModel { get; }
     

    }
}
