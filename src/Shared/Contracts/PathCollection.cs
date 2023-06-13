namespace SSDTLifecycleExtension.Shared.Contracts;

public class PathCollection
{
    [NotNull] public DirectoryPaths Directories { get; }

    [NotNull] public DeploySourcePaths DeploySources { get; }

    [NotNull] public DeployTargetPaths DeployTargets { get; }

    /// <summary>
    ///     Initializes a new <see cref="PathCollection" /> instance for a given set of paths.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="directories" />, <paramref name="deploySources" /> or
    ///     <paramref name="deployTargets" /> are <b>null</b>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Both <paramref name="deployTargets" />.<see cref="DeployTargetPaths.DeployScriptPath" />
    ///     and <paramref name="deployTargets" />.<see cref="DeployTargetPaths.DeployReportPath" /> are <b>null</b>,
    ///     when <paramref name="deploySources" />.<see cref="DeploySourcePaths.PreviousDacpacPath" /> is not <b>null</b>.
    /// </exception>
    public PathCollection([NotNull] DirectoryPaths directories,
                          [NotNull] DeploySourcePaths deploySources,
                          [NotNull] DeployTargetPaths deployTargets)
    {
        Directories = directories ?? throw new ArgumentNullException(nameof(directories));
        DeploySources = deploySources ?? throw new ArgumentNullException(nameof(deploySources));
        DeployTargets = deployTargets ?? throw new ArgumentNullException(nameof(deployTargets));
        if (deploySources.PreviousDacpacPath != null && deployTargets.DeployScriptPath == null && deployTargets.DeployReportPath == null)
            throw new InvalidOperationException($"Either {nameof(DeployTargetPaths.DeployScriptPath)}, "
                                              + $"{nameof(DeployTargetPaths.DeployReportPath)}, or both must be provided, when {nameof(DeploySourcePaths.PreviousDacpacPath)} is provided.");
    }
}