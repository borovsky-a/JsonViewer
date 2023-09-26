using System;
using System.Diagnostics;
using System.Windows.Input;

namespace mop.Configurator
{
    /// <summary>
    ///     Реализация <see cref="ICommand"/>
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Func<object, bool> _canExecuteAction;
        private readonly Action<object> _executeAction;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="executeAction"></param>
        /// <param name="canExecuteAction"></param>
        public RelayCommand(Action<object> executeAction, Func<object, bool> canExecuteAction)
        {
            _executeAction = executeAction;
            _canExecuteAction = canExecuteAction;
        }

        /// <summary>
        ///     Выполняет метод
        /// </summary>
        public void Execute(object parameter)
        {
            _executeAction(parameter);
        }

        /// <summary>
        ///     Предикат. можно ли выполнять команду
        /// </summary>
        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecuteAction(parameter);
        }

        /// <summary>
        ///     <see cref="CanExecuteChanged"/>
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
