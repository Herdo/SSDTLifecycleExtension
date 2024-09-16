namespace SSDTLifecycleExtension.Shared.Models;

public class ScaffoldingStateModel(SqlProject project,
                                   ConfigurationModel configuration,
                                   Version targetVersion,
                                   Func<bool, Task> handleWorkInProgressChanged)
    : BaseStateModel(handleWorkInProgressChanged)
{
    public SqlProject Project { get; } = project;

    public ConfigurationModel Configuration { get; } = configuration;

    public Version TargetVersion { get; } = targetVersion;

    public Version? FormattedTargetVersion { get; set; }

    public PathCollection? Paths { get; set; }
}