﻿namespace SSDTLifecycleExtension.Shared.Contracts;

public class DeployTargetPaths
{
    /// <summary>
    ///     Gets the absolute path of where to create the deployment script, if it should be created.
    /// </summary>
    public string? DeployScriptPath { get; }

    /// <summary>
    ///     Gets the absolute path of where to create the deployment report, if it should be created.
    /// </summary>
    public string? DeployReportPath { get; }

    /// <summary>
    ///     Initializes a new <see cref="DeployTargetPaths" /> instance for the given paths.
    /// </summary>
    /// <param name="deployScriptPath">The optional path of where to create the deploy script.</param>
    /// <param name="deployReportPath">The optional path of where to create the deploy report.</param>
    public DeployTargetPaths(string? deployScriptPath,
        string? deployReportPath)
    {
        DeployScriptPath = deployScriptPath;
        DeployReportPath = deployReportPath;
    }
}