using System;
using System.Windows.Input;

namespace MouseMasterVR
{
    public class RelayCommand<T, U> : ICommand
    {
        private readonly Action<T, U> _execute;
        private readonly Func<T, U, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<T, U> execute) : this(execute, null) { }

        public RelayCommand(Action<T, U> execute, Func<T, U, bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter, (U)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter, (U)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
