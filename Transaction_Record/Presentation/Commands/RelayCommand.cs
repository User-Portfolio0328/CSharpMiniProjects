using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Transaction_Record.Presentation.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private event Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> predicate = null) 
        {
            this._execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this._canExecute = predicate;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) 
            => this._canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => this._execute(parameter);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged 
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);
        public void Execute(object parameter) => _execute((T)parameter);
    }
}
