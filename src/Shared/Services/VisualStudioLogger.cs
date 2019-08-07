namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Threading.Tasks;
    using Contracts.DataAccess;
    using JetBrains.Annotations;

    public class VisualStudioLogger : ILogger
    {
        private readonly IVisualStudioAccess _visualStudioAccess;

        public VisualStudioLogger([NotNull] IVisualStudioAccess visualStudioAccess)
        {
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        }

        private async Task LogInternal(string level, string formattedMessage)
        {
            await _visualStudioAccess.LogToOutputPanelAsync($"{level}: {formattedMessage}");
        }

        Task ILogger.LogCriticalAsync(Exception exception,
                                      string message)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("CRITICAL", $"{message} - {exception}");
        }

        Task ILogger.LogCriticalAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("CRITICAL", message);
        }

        Task ILogger.LogDebugAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("DEBUG", message);
        }

        Task ILogger.LogErrorAsync(Exception exception,
                                   string message)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("ERROR", $"{message} - {exception}");
        }

        Task ILogger.LogErrorAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("ERROR", message);
        }

        Task ILogger.LogInfoAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("INFO", message);
        }

        Task ILogger.LogTraceAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("TRACE", message);
        }

        Task ILogger.LogWarningAsync(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            return LogInternal("WARNING", message);
        }
    }
}