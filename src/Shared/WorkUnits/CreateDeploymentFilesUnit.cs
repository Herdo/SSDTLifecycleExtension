namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class CreateDeploymentFilesUnit(IDacAccess _dacAccess,
                                       IFileSystemAccess _fileSystemAccess,
                                       ILogger _logger)
    : IWorkUnit<ScriptCreationStateModel>
{
    private async Task CreateDeploymentFilesInternal(IStateModel stateModel,
        PathCollection paths,
        ConfigurationModel configuration,
        bool createDocumentationWithScriptCreation)
    {
        var success = await CreateAndPersistDeployFiles(paths, configuration, createDocumentationWithScriptCreation);
        if (!success)
        {
            await _logger.LogErrorAsync("Script creation aborted.");
            stateModel.Result = false;
        }

        stateModel.CurrentState = StateModelState.TriedToCreateDeploymentFiles;
    }

    private async Task<bool> CreateAndPersistDeployFiles(PathCollection paths,
        ConfigurationModel configuration,
        bool createDocumentation)
    {
        await _logger.LogInfoAsync("Creating diff files ...");
        var (success, deployScriptContent, deployReportContent) = await CreateDeployContent(paths, configuration, createDocumentation);
        if (!success)
            return false;

        success = await PersistDeployScript(paths.DeployTargets.DeployScriptPath!, deployScriptContent!);

        if (!success || !createDocumentation)
            return success;

        return await PersistDeployReport(paths.DeployTargets.DeployReportPath!, deployReportContent!);
    }

    private async Task<(bool Success, string? DeployScriptContent, string? DeployReportContent)> CreateDeployContent(PathCollection paths,
        ConfigurationModel configuration,
        bool createDocumentation)
    {
        await _logger.LogDebugAsync($"Previous DACPAC path: \"{paths.DeploySources.PreviousDacpacPath}\"");
        await _logger.LogDebugAsync($"New DACPAC path: \"{paths.DeploySources.NewDacpacPath}\"");
        await _logger.LogDebugAsync($"Publish profile path: \"{paths.DeploySources.PublishProfilePath}\"");
        await _logger.LogDebugAsync($"Current working directory: \"{Environment.CurrentDirectory}\"");

        var result = await _dacAccess.CreateDeployFilesAsync(paths.DeploySources.PreviousDacpacPath!,
            paths.DeploySources.NewDacpacPath,
            paths.DeploySources.PublishProfilePath!,
            true,
            createDocumentation);

        if (result.Errors is null)
        {
            if (!string.IsNullOrEmpty(result.PreDeploymentScript) && !result.DeployScriptContent!.Contains(result.PreDeploymentScript))
            {
                await _logger.LogErrorAsync("Failed to create complete script. Generated script is missing the pre-deployment script.");
                return (false, null, null);
            }

            if (!string.IsNullOrEmpty(result.PostDeploymentScript) && !result.DeployScriptContent!.Contains(result.PostDeploymentScript))
            {
                await _logger.LogErrorAsync("Failed to create complete script. Generated script is missing the post-deployment script.");
                return (false, null, null);
            }

            var valid = await ValidatePublishProfileAgainstConfiguration(result.UsedPublishProfile!, configuration);
            return valid
                ? (true, result.DeployScriptContent, result.DeployReportContent)
                : (false, null, null);
        }

        await _logger.LogErrorAsync("Failed to create script.");
        foreach (var s in result.Errors)
            await _logger.LogErrorAsync(s);

        return (false, null, null);
    }

    private async Task<bool> ValidatePublishProfileAgainstConfiguration(PublishProfile publishProfile,
        ConfigurationModel configuration)
    {
        if (!configuration.RemoveSqlCmdStatements)
            return true;

        var valid = true;
        if (publishProfile.CreateNewDatabase)
        {
            await _logger.LogErrorAsync(
                $"{nameof(PublishProfile.CreateNewDatabase)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
            valid = false;
        }

        if (publishProfile.BackupDatabaseBeforeChanges)
        {
            await _logger.LogErrorAsync(
                $"{nameof(PublishProfile.BackupDatabaseBeforeChanges)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
            valid = false;
        }

        if (publishProfile.ScriptDatabaseOptions)
        {
            await _logger.LogErrorAsync(
                $"{nameof(PublishProfile.ScriptDatabaseOptions)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
            valid = false;
        }

        if (publishProfile.ScriptDeployStateChecks)
        {
            await _logger.LogErrorAsync(
                $"{nameof(PublishProfile.ScriptDeployStateChecks)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
            valid = false;
        }

        return valid;
    }

    private async Task<bool> PersistDeployScript(string deployScriptPath,
        string deployScriptContent)
    {
        try
        {
            await _logger.LogInfoAsync($"Writing deploy script to {deployScriptPath} ...");
            await _fileSystemAccess.WriteFileAsync(deployScriptPath, deployScriptContent);
            return true;
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to write deploy script");
            return false;
        }
    }

    private async Task<bool> PersistDeployReport(string deployReportPath,
        string deployReportContent)
    {
        try
        {
            await _logger.LogInfoAsync($"Writing deploy report to {deployReportPath} ...");
            await _fileSystemAccess.WriteFileAsync(deployReportPath, deployReportContent);
            return true;
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to write deploy report");
            return false;
        }
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.PreviousDacpacPath);
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.NewDacpacPath);
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.PublishProfilePath);
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeployTargets.DeployScriptPath);
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeployTargets.DeployReportPath);

        return CreateDeploymentFilesInternal(stateModel,
            stateModel.Paths,
            stateModel.Configuration,
            stateModel.Configuration.CreateDocumentationWithScriptCreation);
    }
}