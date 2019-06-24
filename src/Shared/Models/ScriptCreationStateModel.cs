namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using JetBrains.Annotations;

    public class ScriptCreationStateModel : BaseStateModel
    {
        [NotNull]
        public SqlProject Project { get; }

        [NotNull]
        public ConfigurationModel Configuration { get; }

        [NotNull]
        public Version PreviousVersion { get; }

        public bool CreateLatest { get; }

        [CanBeNull]
        public Version FormattedTargetVersion { get; set; }

        [CanBeNull]
        public PathCollection Paths { get; set; }

        public bool ArePathsVerified { get; set; }

        public bool HasTriedToBuildProject { get; set; }

        public bool HasTriedToCopyBuildResult { get; set; }

        public bool HasTriedToCreateDeploymentFiles { get; set; }

        public bool HasModifiedDeploymentScript { get; set; }

        public ScriptCreationStateModel([NotNull] SqlProject project,
                                        [NotNull] ConfigurationModel configuration,
                                        [NotNull] Version previousVersion,
                                        bool createLatest,
                                        [NotNull] Func<bool, Task> handleWorkInProgressChanged)
            : base(handleWorkInProgressChanged)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            PreviousVersion = previousVersion ?? throw new ArgumentNullException(nameof(previousVersion));
            CreateLatest = createLatest;
        }
    }
}