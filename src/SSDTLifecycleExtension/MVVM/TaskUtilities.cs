namespace SSDTLifecycleExtension.MVVM
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public static class TaskUtilities
    {
        public static void FireAndForget([NotNull] this Task task,
                                         [NotNull] IAsyncCommand command,
                                         [NotNull] IErrorHandler handler)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            FireAndForgetInternal(task, command, handler);
        }

#pragma warning disable VSTHRD100
        private static async void FireAndForgetInternal(Task task,
                                                        IAsyncCommand command,
                                                        IErrorHandler handler)
#pragma warning restore VSTHRD100
        {
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