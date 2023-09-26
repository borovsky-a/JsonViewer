using mop.Configurator.View;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace mop.Configurator
{
    public class EditValueDialogViewModel : BaseViewModel
    {
        private EditorDialog _dialog;
        private Parameter _parameter;
        private EditorBase _selected;
        public EditValueDialogViewModel(MainViewModel mainViewModel)
        {
            ConfigurationProvider = mainViewModel.ConfigurationProvider;
            MainViewModel = mainViewModel;
        }

        public ICommand SaveCommand => new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);

        private void SaveCommandExecute(object parameter)
        {
            _parameter.Value = EditorManager.Value;
            if(_dialog != null)
            {
                _dialog.Close();              
                _dialog = null;
                _parameter = null;
            }     
        }
        private bool SaveCommandCanExecute(object parameter)
        {
            return true;
        }
        public EditorManager EditorManager
        {
            get { return ConfigurationProvider.Editors; }
        }
        public ConfigurationProvider ConfigurationProvider { get; }
        public MainViewModel MainViewModel { get; }

        public EditorBase Selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }

        public void Show(Parameter parameter)
        {
            _parameter = parameter;
            Selected = null;
            EditorManager.Parameter = parameter;
            EditorManager.Value = parameter.Value;
            var editorType = parameter.EditorType;
            if (!string.IsNullOrEmpty(editorType))
            {
                if (editorType.Equals("password", StringComparison.InvariantCultureIgnoreCase))
                {

                    foreach (var item in EditorManager.Where(o => !o.SuitableNames.Any(x=> x.Equals("password", StringComparison.InvariantCultureIgnoreCase))))
                    {
                        item.IsVisible = false;
                    }
                }
                else if (EditorManager.ShowAll == false)
                {
                    var editor = EditorManager.FirstOrDefault(o => o.SuitableNames.Any(x => x.Equals(editorType, StringComparison.InvariantCultureIgnoreCase)));
                    if (editor != null)
                    {
                        foreach (var item in EditorManager.Where(o => o != editor))
                        {
                            item.IsVisible = false;
                        }
                    }
                    else
                    {
                        foreach (var item in EditorManager)
                        {
                            item.IsVisible = true;
                        }
                    }
                }
                else
                {
                    foreach (var item in EditorManager)
                    {
                        item.IsVisible = true;
                    }
                }
            }           
            if (!string.IsNullOrEmpty(editorType))
                Selected = EditorManager?.FirstOrDefault(o => o.SuitableNames.Any(x => x.Equals(editorType, StringComparison.InvariantCultureIgnoreCase)));           
            if(Selected == null)
                Selected = EditorManager.FirstOrDefault(o => o.IsDefault);

            _dialog = new EditorDialog { Owner = Application.Current.MainWindow , DataContext = this};
            _dialog.ShowDialog();
          
        }
    }
}
