namespace SSDTLifecycleExtension.Shared.Events;

public class ProjectConfigurationChangedEventArgs : EventArgs
{
    public SqlProject Project { get; }

    public ProjectConfigurationChangedEventArgs([NotNull] SqlProject project)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }
}