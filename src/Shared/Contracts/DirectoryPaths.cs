namespace SSDTLifecycleExtension.Shared.Contracts;

public class DirectoryPaths
{
    /// <summary>
    ///     Gets the absolute directory of the *.sqlproj file.
    /// </summary>
    public string ProjectDirectory { get; }

    /// <summary>
    ///     Gets the absolute directory path for the "latest" artifacts.
    /// </summary>
    public string LatestArtifactsDirectory { get; }

    /// <summary>
    ///     Gets the absolute directory path for new artifacts.
    /// </summary>
    public string NewArtifactsDirectory { get; }

    /// <summary>
    ///     Initializes a new <see cref="DirectoryPaths" /> instance for the given directories.
    /// </summary>
    /// <param name="projectDirectory">The directory that contains the SQL project file.</param>
    /// <param name="latestArtifactsDirectory">The directory that contains the "latest" artifacts, if it exists and is filled.</param>
    /// <param name="newArtifactsDirectory">The directory that contains the new artifacts.</param>
    public DirectoryPaths(string projectDirectory,
        string latestArtifactsDirectory,
        string newArtifactsDirectory)
    {
        ProjectDirectory = projectDirectory;
        LatestArtifactsDirectory = latestArtifactsDirectory;
        NewArtifactsDirectory = newArtifactsDirectory;
    }
}