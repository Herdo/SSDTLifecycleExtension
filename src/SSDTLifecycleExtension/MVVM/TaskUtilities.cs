namespace SSDTLifecycleExtension.MVVM
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public static class TaskUtilities
    {
#pragma warning disable VSTHRD100
        public static async void FireAndForget([NotNull] this Task task,
                                               [NotNull] IAsyncCommand command,
                                               [NotNull] IErrorHandler handler)
#pragma warning restore VSTHRD100
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                handler.HandleError(command, e);
            }
        }
    }
}