namespace SSDTLifecycleExtension.MVVM;

public interface IErrorHandler
{
    Task HandleErrorAsync(IAsyncCommand command, Exception exception);
}