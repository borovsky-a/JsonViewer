using FT.Common;
using mop.Configurator.Log;
using mop.Configurator.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

using System.Windows.Input;

namespace mop.Configurator
{
    public class ImportDialogViewModel : BaseViewModel
    {
        private static object _windowLock = new object();
        private static ImportDialog _dialog;
        private ISynchronizationManager _current;
        public ImportDialogViewModel(ConfigurationProvider configurationProvider)
        {
            ConfigurationProvider = configurationProvider;
            foreach (var manager in Managers)
            {
                manager.Initialize();
            }
            var current = Managers.FirstOrDefault(o => o.IsCurrent == true);
            if (current == null)
                current = Managers.First();
            Current = current;
        }
        public ICommand SaveCommand => new RelayCommand(SaveCommandExecute, SaveCommandCanExecute);
        public ICommand CancelCommand => new RelayCommand(CancelCommandExecute, o => true);
        public ConfigurationProvider ConfigurationProvider { get; }

        public ISynchronizationManager Current
        {
            get { return _current; }
            set
            {
                if (_current != value)
                {
                    _current = value;
                    OnPropertyChanged(nameof(Current));
                }
            }
        }
        public IEnumerable<ISynchronizationManager> Managers => ConfigurationProvider.ImportManagers;

        private void CancelCommandExecute(object parameter)
        {
            if (_dialog != null)
            {
                Monitor.Enter(_windowLock);
                if (_dialog != null)
                {
                    _dialog.Close();
                    _dialog = null;
                }
                Monitor.Exit(_windowLock);
            }
        }

        protected virtual void SaveCommandExecute(object parameter)
        {
            if (Current != null)
            {
                try
                {
                    var result = ConfigurationProvider.Refresh(Current);

                    foreach (var item in Managers.Where(o => o != Current))
                    {
                        item.IsCurrent = false;
                        item.SaveSettings(false);
                    }
                    Current.IsCurrent = true;
                    Current.SaveSettings(true);
                    if (result)
                    {
                        if (_dialog != null)
                        {
                            if (_dialog != null)
                            {
                                try
                                {
                                    Monitor.Enter(_windowLock);
                                    if (_dialog != null)
                                    {
                                        _dialog.Close();
                                        _dialog = null;
                                    }
                                }
                                finally
                                {
                                    Monitor.Exit(_windowLock);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteError(this, $"Во время импорта конфигурации произошла ошибка {ex.ToLogString()}")
                        .WriteUIMessage(this, LogLevel.Error, $"Во время импорта конфигурации произошла ошибка {ex.ToLogString()}");
                }
                
            }
        }
        protected virtual bool SaveCommandCanExecute(object parameter)
        {
            if (Current != null)
            {
                return Current.CanExecute(parameter);
            }
            return false;
        }
        public override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Current))
            {
                foreach (var item in Managers.Where(o => o != Current))
                {
                    item.IsCurrent = false;
                }
                Current.IsCurrent = true;
            }
        }
        public void Show()
        {
            if (_dialog == null)
            {
                Monitor.Enter(_windowLock);
                if (_dialog == null)
                {
                    _dialog = new ImportDialog { Owner = Application.Current.MainWindow, DataContext = this };
                    _dialog.Closed += (o, e) => { _dialog = null; };
                    _dialog.Show();
                }
                Monitor.Exit(_windowLock);
            }
        }
    }       
}
