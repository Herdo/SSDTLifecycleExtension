namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using JetBrains.Annotations;

    public class PathCollection
    {
        /// <summary>
        /// Gets the absolute path of the publish profile (xml) used to generate the deployment script and report.
        /// </summary>
        [NotNull]
        public string PublishProfilePath { get; }

        /// <summary>
        /// Gets the absolute directory of the new DACPAC.
        /// </summary>
        [NotNull]
        public string NewDacpacDirectory { get; }

        /// <summary>
        /// Gets the absolute path of the new DACPAC.
        /// </summary>
        [NotNull]
        public string NewDacpacPath { get; }

        /// <summary>
        /// Gets the absolute path of the previous DACPAC, if there is one.
        /// </summary>
        [CanBeNull]
        public string PreviousDacpacPath { get; }

        /// <summary>
        /// Gets the absolute path of where to create the deployment script, if it should be created.
        /// </summary>
        [CanBeNull]
        public string DeployScriptPath { get; }

        /// <summary>
        /// Gets the absolute path of where to create the deployment report, if it should be created.
        /// </summary>
        [CanBeNull]
        public string DeployReportPath { get; }

        /// <summary>
        /// Initializes a new <see cref="PathCollection"/> instance for a given set of paths.
        /// </summary>
        /// <param name="publishProfilePath">The path of the publish profile to use during script or report creation.</param>
        /// <param name="newDacpacDirectory">The directory, where the new DACPAC exists.</param>
        /// <param name="newDacpacPath">The path of the new DACPAC.</param>
        /// <param name="previousDacpacPath">The optional path of the previous DACPAC.</param>
        /// <param name="deployScriptPath">The optional path of where to create the deploy script.</param>
        /// <param name="deployReportPath">The optional path of where to create the deploy report.</param>
        /// <exception cref="ArgumentNullException"><paramref name="publishProfilePath"/>, <paramref name="newDacpacDirectory"/> or <paramref name="newDacpacPath"/> are <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Both <paramref name="deployScriptPath"/> and <paramref name="deployReportPath"/> are <b>null</b>.</exception>
        public PathCollection([NotNull] string publishProfilePath,
                              [NotNull] string newDacpacDirectory,
                              [NotNull] string newDacpacPath,
                              [CanBeNull] string previousDacpacPath,
                              [CanBeNull] string deployScriptPath,
                              [CanBeNull] string deployReportPath)
        {
            PublishProfilePath = publishProfilePath ?? throw new ArgumentNullException(nameof(publishProfilePath));
            NewDacpacDirectory = newDacpacDirectory ?? throw new ArgumentNullException(nameof(newDacpacDirectory));
            NewDacpacPath = newDacpacPath ?? throw new ArgumentNullException(nameof(newDacpacPath));
            PreviousDacpacPath = previousDacpacPath;
            if (deployScriptPath == null && deployReportPath == null)
                throw new InvalidOperationException($"Either {nameof(deployScriptPath)}, {nameof(deployReportPath)}, or both must be provided.");
            DeployScriptPath = deployScriptPath;
            DeployReportPath = deployReportPath;
        }
    }
}