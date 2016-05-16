using System;
using System.Diagnostics;

namespace DiskWatcher
{
    public class DelegateCommand : System.Windows.Input.ICommand
    {
        private readonly Action execute;

        private readonly Func<bool> canExecute;

        public DelegateCommand(Action execute) : this(execute, null) { }

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute));
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter)) execute();
        }
    }
}