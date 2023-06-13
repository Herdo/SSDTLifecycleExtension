namespace SSDTLifecycleExtension.MVVM;

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
            try
            {
                await handler.HandleErrorAsync(command, e).ConfigureAwait(false);
            }
            catch
            {
                // ignored - when handling the exception fails, we don't want to end up in a stack overflow.
            }
        }
    }
}