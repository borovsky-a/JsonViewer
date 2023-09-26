using mop.Configurator.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace mop.Configurator
{
    /// <summary>
    ///     Модель представления для просмотра журнала событий
    /// </summary>
    public class LogViewModel : BaseViewModel
    {
        private readonly ParametersViewModel _parametersViewModel;

        /// <summary>
        ///     ctor
        /// </summary>
        public LogViewModel(MainViewModel mainViewModel)
        {
            _parametersViewModel = mainViewModel.ParametersViewModel;
            Records = new ObservableCollection<LogRecord>();
            CopyCommand = new RelayCommand(o =>
            {
                var sb = new StringBuilder();
                foreach (var item in Records)
                {   
                    sb.AppendLine($"{item.LogLevel} {item}");
                }
                Clipboard.SetText(sb.ToString());
            }, o => true);
            LoggerExtensions.XmlParameterChanged += LoggerExtensions_XmlParameterChanged;
        }

        private void LoggerExtensions_XmlParameterChanged(object sender, ParameterChangedEventArgs e)
        {
            if (e.LogLevel != LogLevel.None)
            {
                Records.Insert(0, new LogRecord(e.LogLevel, e.Message));
            }
        }

      

        /// <summary>
        ///     Коллекция содержит информацию для событий
        /// </summary>
        public ObservableCollection<LogRecord> Records { get; }

        /// <summary>
        ///     Команда очистки журнала
        /// </summary>
        public ICommand CopyCommand { get; }

        /// <summary>
        ///     Удаляет устаревшие записи
        /// </summary>
        public void RemoveDeprecatedRecords()
        {
            if (Records.Count > 2000)
            {
                foreach (var item in Records.OrderByDescending(o => o.Date).Skip(2000).ToArray())
                {
                    Records.Remove(item);
                }
            }
        }

        private void ListBoxAppender_OnLog(LogRecord args)
        {
            Records.Insert(0, args);
        }
    }
}
