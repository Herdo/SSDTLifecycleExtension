#nullable enable

namespace SSDTLifecycleExtension.MVVM;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync();
    bool CanExecute();
    void RaiseCanExecuteChanged();
}