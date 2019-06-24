namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.Models;
    using JetBrains.Annotations;

    public class ScaffoldingStateModel : BaseModel, IStateModel
    {
        [NotNull]
        public SqlProject Project { get; }

        [NotNull]
        public ConfigurationModel Configuration { get; }

        [NotNull]
        public Version TargetVersion { get; }

        [CanBeNull]
        public Version FormattedTargetVersion { get; set; }

        [CanBeNull]
        public PathCollection Paths { get; set; }

        public bool HasTriedToBuildProject { get; set; }

        public bool HasTriedToCopyBuildResult { get; set; }

        public ScaffoldingStateModel([NotNull] SqlProject project,
                                     [NotNull] ConfigurationModel configuration,
                                     [NotNull] Version targetVersion,
                                     [NotNull] Func<bool, Task> handleWorkInProgressChanged)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            TargetVersion = targetVersion ?? throw new ArgumentNullException(nameof(targetVersion));
            HandleWorkInProgressChanged = handleWorkInProgressChanged ?? throw new ArgumentNullException(nameof(handleWorkInProgressChanged));
        }

        public Func<bool, Task> HandleWorkInProgressChanged { get; }

        public bool? Result { get; set; }
    }
}