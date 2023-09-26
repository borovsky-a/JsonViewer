using System.Diagnostics;
using System.Windows.Input;

namespace mop.Configurator
{
    public class ParametersViewModel : BaseViewModel
    {
        private Parameter _selectedParameter;
        public ParametersViewModel(MainViewModel mainViewModel)
        {           
            ConfigurationProvider = mainViewModel.ConfigurationProvider;
            ConfigurationProvider.PropertyChanged += ConfigurationProvider_PropertyChanged;
            MainViewModel = mainViewModel;
        }

        private void ConfigurationProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ConfigurationProvider.Parameters))
            {
                OnPropertyChanged(nameof(Parameters));
            }
        }

        public string Filter
        {
            get { return ConfigurationProvider?.Filer; }
            set
            {
                if (ConfigurationProvider != null && ConfigurationProvider.Filer != value)
                {
                    ConfigurationProvider.Filer = value;
                    OnPropertyChanged(nameof(Filter));
                }
            }
        }

        public ParameterCollection Parameters
        {
            get { return ConfigurationProvider.Parameters;  }
        }
       
        public ICommand EditCommand => new RelayCommand(EditCommandExecute, AllowCanExecute);

        public Parameter SelectedParameter
        {
            get { return _selectedParameter; }
            set
            {
                if (_selectedParameter != value)
                {
                    _selectedParameter = value;
                    OnPropertyChanged(nameof(SelectedParameter));
                }
            }
        }

        public ConfigurationProvider ConfigurationProvider { get; }
        public MainViewModel MainViewModel { get; }

        
        private void EditCommandExecute(object parameter)
        {
            var param = parameter as Parameter;
            if(param != null)
                new EditValueDialogViewModel(MainViewModel).Show(param);
        }
        [DebuggerStepThrough]
        private bool AllowCanExecute(object parameter)
        {
            return true;
        }
    }
}
