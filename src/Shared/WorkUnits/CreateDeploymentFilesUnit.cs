namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using Contracts.Models;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class CreateDeploymentFilesUnit : IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IDacAccess _dacAccess;
        [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
        [NotNull] private readonly ILogger _logger;

        public CreateDeploymentFilesUnit([NotNull] IDacAccess dacAccess,
                                         [NotNull] IFileSystemAccess fileSystemAccess,
                                         [NotNull] ILogger logger)
        {
            _dacAccess = dacAccess ?? throw new ArgumentNullException(nameof(dacAccess));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task CreateDeploymentFilesInternal(IStateModel stateModel,
                                                         PathCollection paths,
                                                         ConfigurationModel configuration,
                                                         bool createDocumentationWithScriptCreation)
        {
            var success = await CreateAndPersistDeployFiles(paths, configuration, createDocumentationWithScriptCreation);
            if (!success)
            {
                await _logger.LogAsync("ERROR: Script creation aborted.");
                stateModel.Result = false;
            }

            stateModel.CurrentState = StateModelState.TriedToCreateDeploymentFiles;
        }

        private async Task<bool> CreateAndPersistDeployFiles(PathCollection paths,
                                                             ConfigurationModel configuration,
                                                             bool createDocumentation)
        {
            await _logger.LogAsync("Creating diff files ...");
            var result = await CreateDeployContent(paths, configuration, createDocumentation);
            if (!result.Success)
                return false;

            var success = await PersistDeployScript(paths.DeployTargets.DeployScriptPath, result.DeployScriptContent);

            if (!success || !createDocumentation)
                return success;

            return await PersistDeployReport(paths.DeployTargets.DeployReportPath, result.DeployReportContent);
        }

        private async Task<(bool Success, string DeployScriptContent, string DeployReportContent)> CreateDeployContent(PathCollection paths,
                                                                                                                       ConfigurationModel configuration,
                                                                                                                       bool createDocumentation)
        {
            var result = await _dacAccess.CreateDeployFilesAsync(paths.DeploySources.PreviousDacpacPath,
                                                                 paths.DeploySources.NewDacpacPath,
                                                                 paths.DeploySources.PublishProfilePath,
                                                                 true,
                                                                 createDocumentation,
                                                                 profile => ValidatePublishProfileAgainstConfiguration(profile, configuration));

            if (result.Errors == null)
            {
                if (!string.IsNullOrEmpty(result.PreDeploymentScript) && !result.DeployScriptContent.Contains(result.PreDeploymentScript))
                {
                    await _logger.LogAsync("ERROR: Failed to create complete script. Generated script is missing the pre-deployment script.");
                    return (false, null, null);
                }

                if (!string.IsNullOrEmpty(result.PostDeploymentScript) && !result.DeployScriptContent.Contains(result.PostDeploymentScript))
                {
                    await _logger.LogAsync("ERROR: Failed to create complete script. Generated script is missing the post-deployment script.");
                    return (false, null, null);
                }

                return (true, result.DeployScriptContent, result.DeployReportContent);
            }

            await _logger.LogAsync("ERROR: Failed to create script:");
            foreach (var s in result.Errors)
                await _logger.LogAsync(s);

            return (false, null, null);
        }

        private async Task<bool> ValidatePublishProfileAgainstConfiguration(PublishProfile publishProfile,
                                                                            ConfigurationModel configuration)
        {
            if (!configuration.RemoveSqlCmdStatements)
                return true;

            if (publishProfile.CreateNewDatabase)
            {
                await _logger.LogAsync($"{nameof(PublishProfile.CreateNewDatabase)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
                return false;
            }

            if (publishProfile.BackupDatabaseBeforeChanges)
            {
                await _logger.LogAsync($"{nameof(PublishProfile.BackupDatabaseBeforeChanges)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
                return false;
            }

            if (publishProfile.ScriptDatabaseOptions)
            {
                await _logger.LogAsync($"{nameof(PublishProfile.ScriptDatabaseOptions)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
                return false;
            }

            if (publishProfile.ScriptDeployStateChecks)
            {
                await _logger.LogAsync($"{nameof(PublishProfile.ScriptDeployStateChecks)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true.");
                return false;
            }

            return true;
        }

        private async Task<bool> PersistDeployScript(string deployScriptPath,
                                                     string deployScriptContent)
        {
            try
            {
                await _logger.LogAsync($"Writing deploy script to {deployScriptPath} ...");
                await _fileSystemAccess.WriteFileAsync(deployScriptPath, deployScriptContent);
                return true;
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to write deploy script: {e.Message}");
                return false;
            }
        }

        private async Task<bool> PersistDeployReport(string deployReportPath,
                                                     string deployReportContent)
        {
            try
            {
                await _logger.LogAsync($"Writing deploy report to {deployReportPath} ...");
                await _fileSystemAccess.WriteFileAsync(deployReportPath, deployReportContent);
                return true;
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to write deploy report: {e.Message}");
                return false;
            }
        }

        Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                      CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return CreateDeploymentFilesInternal(stateModel,
                                                 stateModel.Paths,
                                                 stateModel.Configuration,
                                                 stateModel.Configuration.CreateDocumentationWithScriptCreation);
        }
    }
}