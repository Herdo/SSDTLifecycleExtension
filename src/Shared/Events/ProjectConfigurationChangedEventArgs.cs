namespace SSDTLifecycleExtension.Shared.Events
{
    using System;
    using Contracts;
    using JetBrains.Annotations;

    public class ProjectConfigurationChangedEventArgs : EventArgs
    {
        public SqlProject Project { get; }

        public ProjectConfigurationChangedEventArgs([NotNull] SqlProject project)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
        }
    }
}