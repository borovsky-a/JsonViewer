using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace mop.Configurator
{
    public class ConfigurationProvider : BaseViewModel
    {
        //private ParameterSection _section;
        private IConfigurationManager _currentConfigurationManager;
        private string _filter;

        public ConfigurationProvider()
        {
            RegisteredConfigurationManagers = new ObservableCollection<IConfigurationManager>();
            RegisteredConfigurationManagers.CollectionChanged += (o, e) => 
            {
                if(e.NewItems.Count > 0)
                {
                    if (RegisteredConfigurationManagers.Count == 1)
                    {
                        CurrentConfigurationManager = (IConfigurationManager)e.NewItems[0];
                    }
                }                
            };
            ParameterCache = new Dictionary<string, Parameter>();
            Parameters = new ParameterCollection();
            Section = new ParameterSection("", this);
        }
        public bool UseCheckBoxes { get; set; }
        public bool UseFilter { get; set; }
        public bool IgnoreFilterFlag { get; set; }
        public Dictionary<string, Parameter> ParameterCache { get; }
        public ParameterCollection Parameters { get; private set; }
        //public ParameterSection Section
        //{
        //    get { return _section; }
        //    set
        //    {
        //        if(_section != value)
        //        {
        //            _section = value;
        //            OnPropertyChanged(nameof(Section));
        //        }
        //    }
        //}

        public IConfigurationManager CurrentConfigurationManager
        {
            get { return _currentConfigurationManager; }
            set
            {
                if(_currentConfigurationManager != value)
                {
                    _currentConfigurationManager = value;
                    OnPropertyChanged(nameof(CurrentConfigurationManager));
                }
            }
        }
        public ObservableCollection<IConfigurationManager> RegisteredConfigurationManagers { get; set; }

        public void Refresh()
        {
            Section.Refresh();
            //CurrentConfigurationManager.Refresh();

        }
    }
}
