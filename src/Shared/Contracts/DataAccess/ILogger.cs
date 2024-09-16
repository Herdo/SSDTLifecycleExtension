namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface ILogger
{
    string DocumentationBaseUrl { get; }

    Task LogCriticalAsync(Exception exception,
        string message);

    Task LogCriticalAsync(string message);

    Task LogErrorAsync(Exception exception,
        string message);

    Task LogErrorAsync(string message);

    Task LogWarningAsync(string message);

    Task LogInfoAsync(string message);

    Task LogDebugAsync(string message);

    Task LogTraceAsync(string message);
}