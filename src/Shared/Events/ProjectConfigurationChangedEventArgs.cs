namespace SSDTLifecycleExtension.Shared.Events;

public class ProjectConfigurationChangedEventArgs(SqlProject project)
    : EventArgs
{
    public SqlProject Project { get; } = project;
}