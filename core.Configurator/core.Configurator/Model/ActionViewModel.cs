using System.Linq;
using System.Windows.Input;

namespace mop.Configurator
{
    public class ActionViewModel : BaseViewModel
    {
        private readonly MainViewModel _mainViewModel;

        public ActionViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
        public ICommand ImportDialogCommand => new RelayCommand(ImportDialogCommandExecute, o => true);
        public ICommand ExportDialogCommand => new RelayCommand(ExportDialogCommandExecute, ExportDialogCommandCanExecute);
        public ICommand RefreshCommand => new RelayCommand(RefreshCommandExecute, RefreshCommandCanExecute);
        public string Filter
        {
            get { return _mainViewModel.ConfigurationProvider?.Filer; }
            set
            {
                _mainViewModel.ConfigurationProvider.Filer = value;
                OnPropertyChanged(nameof(Filter));
            }
        }
        protected virtual void ExportDialogCommandExecute(object parameter)
        {
            var dialog = new ExportDialogViewModel(_mainViewModel.ConfigurationProvider);
            dialog.Show();
        }
        protected virtual bool ExportDialogCommandCanExecute(object parameter)
        {
            return _mainViewModel.ConfigurationProvider.Parameters != null && _mainViewModel.ConfigurationProvider.XDocument != null;
        }
        protected virtual void ImportDialogCommandExecute(object parameter)
        {
            var dialog = new ImportDialogViewModel(_mainViewModel.ConfigurationProvider);
            dialog.Show();
        }
        protected virtual void RefreshCommandExecute(object parameter)
        {
            var manager = GetCurrentImportManager();
            if (manager != null)
                _mainViewModel.ConfigurationProvider.Refresh(manager);
        }
        protected virtual bool RefreshCommandCanExecute(object parameter)
        {
            var manager = GetCurrentImportManager();
            return manager != null && manager.CanExecute(parameter);
        }

        private ISynchronizationManager GetCurrentImportManager()
        {
            var currentImportManager =
                 _mainViewModel.ConfigurationProvider.ImportManagers.FirstOrDefault(o=> o.IsCurrent);
            return currentImportManager;
        }
    }
}
