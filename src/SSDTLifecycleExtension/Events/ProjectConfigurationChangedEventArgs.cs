namespace SSDTLifecycleExtension.Events
{
    using System;
    using Annotations;
    using EnvDTE;

    public class ProjectConfigurationChangedEventArgs : EventArgs
    {
        public Project Project { get; }

        public ProjectConfigurationChangedEventArgs([NotNull] Project project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }
    }
}