namespace SSDTLifecycleExtension.Shared.Contracts;

public class PathCollection
{
    public DirectoryPaths Directories { get; }

    public DeploySourcePaths DeploySources { get; }

    public DeployTargetPaths DeployTargets { get; }

    /// <summary>
    ///     Initializes a new <see cref="PathCollection" /> instance for a given set of paths.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Both <paramref name="deployTargets" />.<see cref="DeployTargetPaths.DeployScriptPath" />
    ///     and <paramref name="deployTargets" />.<see cref="DeployTargetPaths.DeployReportPath" /> are <b>null</b>,
    ///     when <paramref name="deploySources" />.<see cref="DeploySourcePaths.PreviousDacpacPath" /> is not <b>null</b>.
    /// </exception>
    public PathCollection(DirectoryPaths directories,
        DeploySourcePaths deploySources,
        DeployTargetPaths deployTargets)
    {
        Directories = directories;
        DeploySources = deploySources;
        DeployTargets = deployTargets;
        if (deploySources.PreviousDacpacPath is not null && deployTargets.DeployScriptPath is null && deployTargets.DeployReportPath is null)
            throw new InvalidOperationException($"Either {nameof(DeployTargetPaths.DeployScriptPath)}, "
                + $"{nameof(DeployTargetPaths.DeployReportPath)}, or both must be provided, when {nameof(DeploySourcePaths.PreviousDacpacPath)} is provided.");
    }
}