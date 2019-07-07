namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using JetBrains.Annotations;

    public class DeploySourcePaths
    {
        /// <summary>
        /// Gets the absolute path of the new DACPAC.
        /// </summary>
        [NotNull]
        public string NewDacpacPath { get; }

        /// <summary>
        /// Gets the absolute path of the publish profile (xml) used to generate the deployment script and report.
        /// </summary>
        [NotNull]
        public string PublishProfilePath { get; }

        /// <summary>
        /// Gets the absolute path of the previous DACPAC, if there is one.
        /// </summary>
        [CanBeNull]
        public string PreviousDacpacPath { get; }

        /// <summary>
        /// Initializes a new <see cref="DeploySourcePaths"/> instance for the given paths.
        /// </summary>
        /// <param name="newDacpacPath">The path of the new DACPAC.</param>
        /// <param name="publishProfilePath">The path of the publish profile to use during script or report creation.</param>
        /// <param name="previousDacpacPath">The optional path of the previous DACPAC.</param>
        /// <exception cref="ArgumentNullException"><paramref name="newDacpacPath"/> or <paramref name="publishProfilePath"/> are <b>null</b>.</exception>
        public DeploySourcePaths([NotNull] string newDacpacPath,
                                 [NotNull] string publishProfilePath,
                                 [CanBeNull] string previousDacpacPath)
        {
            NewDacpacPath = newDacpacPath ?? throw new ArgumentNullException(nameof(newDacpacPath));
            PublishProfilePath = publishProfilePath ?? throw new ArgumentNullException(nameof(publishProfilePath));
            PreviousDacpacPath = previousDacpacPath;
        }
    }
}