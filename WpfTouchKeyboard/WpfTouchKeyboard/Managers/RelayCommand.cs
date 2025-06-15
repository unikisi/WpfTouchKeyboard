using System.Windows.Input;

namespace WpfTouchKeyboard.Managers
{
    public class RelayCommand<T>(Action<T> execute, Predicate<T>? canExecute = null) : ICommand
    {
        private readonly Action<T> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute((T)parameter!);
        }

        public void Execute(object? parameter)
        {
            _execute((T)parameter!);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
