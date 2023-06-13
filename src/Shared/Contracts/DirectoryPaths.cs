namespace SSDTLifecycleExtension.Shared.Contracts;

public class DirectoryPaths
{
    /// <summary>
    ///     Gets the absolute directory of the *.sqlproj file.
    /// </summary>
    [NotNull]
    public string ProjectDirectory { get; }

    /// <summary>
    ///     Gets the absolute directory path for the "latest" artifacts.
    /// </summary>
    [NotNull]
    public string LatestArtifactsDirectory { get; }

    /// <summary>
    ///     Gets the absolute directory path for new artifacts.
    /// </summary>
    [NotNull]
    public string NewArtifactsDirectory { get; }

    /// <summary>
    ///     Initializes a new <see cref="DirectoryPaths" /> instance for the given directories.
    /// </summary>
    /// <param name="projectDirectory">The directory that contains the SQL project file.</param>
    /// <param name="latestArtifactsDirectory">The directory that contains the "latest" artifacts, if it exists and is filled.</param>
    /// <param name="newArtifactsDirectory">The directory that contains the new artifacts.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="projectDirectory" />,
    ///     <paramref name="latestArtifactsDirectory" /> or <paramref name="newArtifactsDirectory" /> are <b>null</b>.
    /// </exception>
    public DirectoryPaths([NotNull] string projectDirectory,
                          [NotNull] string latestArtifactsDirectory,
                          [NotNull] string newArtifactsDirectory)
    {
        ProjectDirectory = projectDirectory ?? throw new ArgumentNullException(nameof(projectDirectory));
        LatestArtifactsDirectory = latestArtifactsDirectory ?? throw new ArgumentNullException(nameof(latestArtifactsDirectory));
        NewArtifactsDirectory = newArtifactsDirectory ?? throw new ArgumentNullException(nameof(newArtifactsDirectory));
    }
}