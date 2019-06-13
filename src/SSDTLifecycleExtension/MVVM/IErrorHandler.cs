namespace SSDTLifecycleExtension.MVVM
{
    using System;

    public interface IErrorHandler
    {
        void HandleError(IAsyncCommand command, Exception exception);
    }
}