namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using JetBrains.Annotations;

    public class ScaffoldingStateModel : BaseStateModel
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

        public ScaffoldingStateModel([NotNull] SqlProject project,
                                     [NotNull] ConfigurationModel configuration,
                                     [NotNull] Version targetVersion,
                                     [NotNull] Func<bool, Task> handleWorkInProgressChanged)
            : base(handleWorkInProgressChanged)
        {
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            TargetVersion = targetVersion ?? throw new ArgumentNullException(nameof(targetVersion));
        }
    }
}