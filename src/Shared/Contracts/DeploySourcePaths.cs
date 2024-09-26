namespace SSDTLifecycleExtension.Shared.Contracts;

public class DeploySourcePaths
{
    /// <summary>
    ///     Gets the absolute path of the new DACPAC.
    /// </summary>
    public string NewDacpacPath { get; }

    /// <summary>
    ///     Gets the absolute path of the publish profile (xml) used to generate the deployment script and report.
    /// </summary>
    public string? PublishProfilePath { get; }

    /// <summary>
    ///     Gets the absolute path of the previous DACPAC, if there is one.
    /// </summary>
    public string? PreviousDacpacPath { get; }

    /// <summary>
    ///     Initializes a new <see cref="DeploySourcePaths" /> instance for the given paths.
    /// </summary>
    /// <param name="newDacpacPath">The path of the new DACPAC.</param>
    /// <param name="publishProfilePath">The path of the publish profile to use during script or report creation.</param>
    /// <param name="previousDacpacPath">The optional path of the previous DACPAC.</param>
    public DeploySourcePaths(string newDacpacPath,
        string? publishProfilePath,
        string? previousDacpacPath)
    {
        NewDacpacPath = newDacpacPath;
        PublishProfilePath = publishProfilePath;
        PreviousDacpacPath = previousDacpacPath;
    }
}