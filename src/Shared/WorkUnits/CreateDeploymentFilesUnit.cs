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
                                                         bool createDocumentationWithScriptCreation)
        {
            var success = await CreateAndPersistDeployFiles(paths, createDocumentationWithScriptCreation);
            if (!success)
            {
                await _logger.LogAsync("ERROR: Script creation aborted.");
                stateModel.Result = false;
            }

            stateModel.CurrentState = StateModelState.TriedToCreateDeploymentFiles;
        }

        private async Task<bool> CreateAndPersistDeployFiles(PathCollection paths,
                                                             bool createDocumentation)
        {
            await _logger.LogAsync("Creating diff files ...");
            var result = await CreateDeployContent(paths, createDocumentation);
            if (!result.Success)
                return false;

            var success = await PersistDeployScript(paths.DeployTargets.DeployScriptPath, result.DeployScriptContent);

            if (!success || !createDocumentation)
                return success;

            return await PersistDeployReport(paths.DeployTargets.DeployReportPath, result.DeployReportContent);
        }

        private async Task<(bool Success, string DeployScriptContent, string DeployReportContent)> CreateDeployContent(PathCollection paths,
                                                                                                                       bool createDocumentation)
        {
            var (deployScriptContent, deployReportContent, errors) = await _dacAccess.CreateDeployFilesAsync(paths.DeploySources.PreviousDacpacPath,
                                                                                                             paths.DeploySources.NewDacpacPath,
                                                                                                             paths.DeploySources.PublishProfilePath,
                                                                                                             true,
                                                                                                             createDocumentation);

            if (errors == null)
                return (true, deployScriptContent, deployReportContent);

            await _logger.LogAsync("ERROR: Failed to create script:");
            foreach (var s in errors)
                await _logger.LogAsync(s);

            return (false, null, null);
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

            return CreateDeploymentFilesInternal(stateModel, stateModel.Paths, stateModel.Configuration.CreateDocumentationWithScriptCreation);
        }
    }
}