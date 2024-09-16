#nullable enable

namespace SSDTLifecycleExtension.MVVM;

public class AsyncCommand(Func<Task> _execute,
        Func<bool> _canExecute,
        IErrorHandler _errorHandler)
    : IAsyncCommand
{
    public event EventHandler? CanExecuteChanged;

    private bool _isExecuting;

    public bool CanExecute()
    {
        return !_isExecuting && _canExecute.Invoke();
    }

    public async Task ExecuteAsync()
    {
        if (CanExecute())
        {
            try
            {
                _isExecuting = true;
                RaiseCanExecuteChanged();
                await _execute();
            }
            finally
            {
                _isExecuting = false;
            }
        }

        RaiseCanExecuteChanged();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    bool ICommand.CanExecute(object parameter)
    {
        return CanExecute();
    }

    void ICommand.Execute(object parameter)
    {
        ExecuteAsync().FireAndForget(this, _errorHandler);
    }
}