namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface ILogger
{
    string DocumentationBaseUrl { get; }

    Task LogCriticalAsync([NotNull] Exception exception,
                          [NotNull] string message);

    Task LogCriticalAsync([NotNull] string message);

    Task LogErrorAsync([NotNull] Exception exception,
                       [NotNull] string message);

    Task LogErrorAsync([NotNull] string message);

    Task LogWarningAsync([NotNull] string message);

    Task LogInfoAsync([NotNull] string message);

    Task LogDebugAsync([NotNull] string message);

    Task LogTraceAsync([NotNull] string message);
}