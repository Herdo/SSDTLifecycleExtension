namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using JetBrains.Annotations;

    public class PathCollection
    {
        /// <summary>
        /// Gets the absolute directory of the *.sqlproj file.
        /// </summary>
        [NotNull]
        public string ProjectDirectory { get; }

        /// <summary>
        /// Gets the absolute path of the publish profile (xml) used to generate the deployment script and report.
        /// </summary>
        [NotNull]
        public string PublishProfilePath { get; }

        /// <summary>
        /// Gets the absolute directory path for the "latest" artifacts.
        /// </summary>
        [NotNull]
        public string LatestArtifactsDirectory { get; }

        /// <summary>
        /// Gets the absolute directory path for new artifacts.
        /// </summary>
        [NotNull]
        public string NewArtifactsDirectory { get; }

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
        /// <param name="projectDirectory">The directory that contains the SQL project file.</param>
        /// <param name="publishProfilePath">The path of the publish profile to use during script or report creation.</param>
        /// <param name="latestArtifactsDirectory">The directory that contains the "latest" artifacts, if it exists and is filled.</param>
        /// <param name="newArtifactsDirectory">The directory that contains the new artifacts.</param>
        /// <param name="newDacpacPath">The path of the new DACPAC.</param>
        /// <param name="previousDacpacPath">The optional path of the previous DACPAC.</param>
        /// <param name="deployScriptPath">The optional path of where to create the deploy script.</param>
        /// <param name="deployReportPath">The optional path of where to create the deploy report.</param>
        /// <exception cref="ArgumentNullException"><paramref name="publishProfilePath"/>, <paramref name="newArtifactsDirectory"/> or <paramref name="newDacpacPath"/> are <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">Both <paramref name="deployScriptPath"/> and <paramref name="deployReportPath"/> are <b>null</b>,
        /// when <paramref name="previousDacpacPath"/> is not <b>null</b>.</exception>
        public PathCollection([NotNull] string projectDirectory,
                              [NotNull] string publishProfilePath,
                              [NotNull] string latestArtifactsDirectory,
                              [NotNull] string newArtifactsDirectory,
                              [NotNull] string newDacpacPath,
                              [CanBeNull] string previousDacpacPath,
                              [CanBeNull] string deployScriptPath,
                              [CanBeNull] string deployReportPath)
        {
            ProjectDirectory = projectDirectory ?? throw new ArgumentNullException(nameof(projectDirectory));
            PublishProfilePath = publishProfilePath ?? throw new ArgumentNullException(nameof(publishProfilePath));
            LatestArtifactsDirectory = latestArtifactsDirectory ?? throw new ArgumentNullException(nameof(latestArtifactsDirectory));
            NewArtifactsDirectory = newArtifactsDirectory ?? throw new ArgumentNullException(nameof(newArtifactsDirectory));
            NewDacpacPath = newDacpacPath ?? throw new ArgumentNullException(nameof(newDacpacPath));
            PreviousDacpacPath = previousDacpacPath;
            if (previousDacpacPath != null && deployScriptPath == null && deployReportPath == null)
                throw new InvalidOperationException($"Either {nameof(deployScriptPath)}, {nameof(deployReportPath)}, or both must be provided, when {nameof(previousDacpacPath)} is provided.");
            DeployScriptPath = deployScriptPath;
            DeployReportPath = deployReportPath;
        }
    }
}