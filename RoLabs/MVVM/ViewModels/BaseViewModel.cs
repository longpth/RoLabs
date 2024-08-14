using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Rolabs.MVVM.ViewModels;

public class RelayCommand : ICommand
{
    private Action<object> execute;
    private Func<object, bool> canExecute;
    private event EventHandler canExecuteChanged;

    public event EventHandler CanExecuteChanged
    {
        add { canExecuteChanged += value; }
        remove { canExecuteChanged -= value; }
    }

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public bool CanExecute(object parameter)
    {
        return canExecute == null || canExecute(parameter);
    }

    public void Execute(object parameter)
    {
        execute(parameter);
    }

    public void RaiseCanExecuteChanged()
    {
        canExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class BaseViewModel : INotifyPropertyChanged
{
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        onChanged?.Invoke();
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

}