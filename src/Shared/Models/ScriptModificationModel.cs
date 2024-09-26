namespace SSDTLifecycleExtension.Shared.Models;

public class ScriptModificationModel
{
    private string _currentScript;

    public string CurrentScript
    {
        get => _currentScript;
        set
        {
            if (value == _currentScript)
                return;
            _currentScript = value ?? throw new ArgumentNullException(nameof(value));
        }
    }

    public SqlProject Project { get; }

    public ConfigurationModel Configuration { get; }

    public PathCollection Paths { get; }

    public Version PreviousVersion { get; }

    public bool CreateLatest { get; }

    public ScriptModificationModel(string initialScript,
       SqlProject project,
       ConfigurationModel configuration,
       PathCollection paths,
       Version previousVersion,
       bool createLatest)
    {
        _currentScript = initialScript;
        Project = project;
        Configuration = configuration;
        Paths = paths;
        PreviousVersion = previousVersion;
        CreateLatest = createLatest;
    }
}