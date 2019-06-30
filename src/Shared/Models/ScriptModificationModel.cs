namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using Contracts;
    using JetBrains.Annotations;

    public class ScriptModificationModel
    {
        private string _currentScript;

        [NotNull]
        public string CurrentScript
        {
            get => _currentScript;
            set
            {
                if (value == _currentScript) return;
                _currentScript = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        [NotNull]
        public SqlProject Project { get; }

        [NotNull]
        public ConfigurationModel Configuration { get; }

        [NotNull]
        public PathCollection Paths { get; }

        [NotNull]
        public Version PreviousVersion { get; }

        public bool CreateLatest { get; }

        public ScriptModificationModel([NotNull] string initialScript,
                                       [NotNull] SqlProject project,
                                       [NotNull] ConfigurationModel configuration,
                                       [NotNull] PathCollection paths,
                                       [NotNull] Version previousVersion,
                                       bool createLatest)
        {
            CurrentScript = initialScript ?? throw new ArgumentNullException(nameof(initialScript));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Paths = paths ?? throw new ArgumentNullException(nameof(paths));
            PreviousVersion = previousVersion ?? throw new ArgumentNullException(nameof(previousVersion));
            CreateLatest = createLatest;
        }
    }
}