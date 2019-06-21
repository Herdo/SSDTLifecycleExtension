namespace SSDTLifecycleExtension.MVVM
{
    using System;
    using System.Threading.Tasks;

    public interface IErrorHandler
    {
        Task HandleErrorAsync(IAsyncCommand command, Exception exception);
    }
}