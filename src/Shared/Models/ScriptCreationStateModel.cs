namespace SSDTLifecycleExtension.Shared.Models;

public class ScriptCreationStateModel : BaseStateModel
{
    public SqlProject Project { get; }

    public ConfigurationModel Configuration { get; }

    public Version PreviousVersion { get; }

    public bool CreateLatest { get; }

    public Version? FormattedTargetVersion { get; set; }

    public PathCollection? Paths { get; set; }

    public ScriptCreationStateModel(SqlProject project,
        ConfigurationModel configuration,
        Version previousVersion,
        bool createLatest,
        Func<bool, Task> handleWorkInProgressChanged)
        : base(handleWorkInProgressChanged)
    {
        Project = project;
        Configuration = configuration;
        PreviousVersion = previousVersion;
        CreateLatest = createLatest;
    }
}