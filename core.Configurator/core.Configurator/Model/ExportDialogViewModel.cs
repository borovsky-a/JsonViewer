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
    public class ExportDialogViewModel : BaseViewModel
    {
        private static ExportDialog _dialog;
        private static object _windowLock = new object(); 
        private ISynchronizationManager _current;
      
        public ExportDialogViewModel(ConfigurationProvider configurationProvider)
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
        public ICommand CompareCommand => new RelayCommand(CompareCommandExecute,o =>true);
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
        public IEnumerable<ISynchronizationManager> Managers => ConfigurationProvider.ExportManagers;

        
        protected virtual void CancelCommandExecute(object parameter)
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
                    var currentImport = ConfigurationProvider.ImportManagers.Where(o => o.IsCurrent).FirstOrDefault()?.ApplicationName;
                    if (Current.ApplicationName != currentImport)
                    {
                        var dr = MessageBox.Show($"Выполняется попытка сохранения настроек от '{currentImport}' для '{Current.ApplicationName}'. {Environment.NewLine}Подтвердите действие.", "Внимание!", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                        if (dr == MessageBoxResult.No)
                            return;                       
                    }
                    var result = ConfigurationProvider.Save(Current);
                  
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
                catch (Exception ex)
                {
                    Logger.WriteError(this, $"Во время сохранения конфигурации произошла ошибка {ex.ToLogString()}")
                        .WriteUIMessage(this, LogLevel.Error, $"Во время сохранения конфигурации произошла ошибка {ex.ToLogString()}");
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
        protected virtual void CompareCommandExecute(object parameter)
        {
            if (Current == null)
                return;
            var manager = Current.Clone();
            var doc = manager.Import(true);
            var builder = new ParameterCollectionBuilder(ConfigurationProvider);
            var parameters = builder.Create(doc);
            var parametersHashTable = parameters.ToDictionary();

            string[] allowNames = new string[] { "IntValue", "StringValue", "DateTimeValue" };

            var existsParameterValues = parametersHashTable.Select(o => o.Value).Where(o => allowNames.Any(c=> c == o.Name));
            var newParameters = ConfigurationProvider.ParameterCache.Select(o => o.Value).Where(o => allowNames.Any(c => c == o.Name) && o.Parent  != null);
            var messages = new List<string>();
            foreach (var newParameter in newParameters)
            {
                var p = existsParameterValues.FirstOrDefault(o => o.XPath == newParameter.XPath);
                if(p != null)
                {
                    if(p.Value != newParameter.Value)
                    {
                        messages.Add($"Изменено значение параметра '{p.Parent?.DisplayName}' с '{p.Value}' на '{newParameter.Value}'. ");                        
                    }
                }
                else
                {
                    messages.Add($"Добавлен параметр '{newParameter.Parent?.DisplayName}'. Значение: '{newParameter.Value}'.");                   
                }
            }
            foreach (var existsParameter in existsParameterValues)
            {
                if(!newParameters.Any(o=> o.XPath == existsParameter.XPath))
                {
                    messages.Add($"Удален параметр '{existsParameter.Parent?.DisplayName}'. Значение: '{existsParameter.Value}'.");                   
                }
            }
            if(messages.Count() == 0)
            {
                var msg = "<< Конфигурация идентична. >>";
                Logger.WriteWarning(this, msg)
                     .WriteUIMessage(this, LogLevel.Warning, msg);
            }
            else
            {
                messages.Insert(0, "<<< Начало отчета о раличии значений в конфигурации приложения >>>");
                messages.Add("<< Конец отчета о раличии значений в конфигурации приложения >>");
                foreach (var message in messages)
                {
                    Logger.WriteWarning(this, message)
                      .WriteUIMessage(this, LogLevel.Warning, message);
                }
            }
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
                    _dialog = new ExportDialog { Owner = Application.Current.MainWindow, DataContext = this };
                    _dialog.Closed += (o, e) => { _dialog = null; };
                    _dialog.Show();
                }
                Monitor.Exit(_windowLock);
            }
        }       
    }   
}
