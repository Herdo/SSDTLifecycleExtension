namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
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

        private async Task<bool> CreateScriptAsync(PathCollection paths,
                                                   bool createDocumentation)
        {
            await _logger.LogAsync("Creating diff files ...");
            var result = await _dacAccess.CreateDeployFilesAsync(paths.PreviousDacpacPath,
                                                                 paths.NewDacpacPath,
                                                                 paths.PublishProfilePath,
                                                                 true,
                                                                 createDocumentation);

            if (result.Errors != null)
            {
                await _logger.LogAsync("ERROR: Failed to create script:");
                foreach (var s in result.Errors)
                    await _logger.LogAsync(s);

                return false;
            }

            var success = true;
            try
            {
                await _logger.LogAsync($"Writing deploy script to {paths.DeployScriptPath} ...");
                await _fileSystemAccess.WriteFileAsync(paths.DeployScriptPath, result.DeployScriptContent);
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to write deploy script: {e.Message}");
                success = false;
            }

            if (!createDocumentation)
                return success;

            try
            {
                await _logger.LogAsync($"Writing deploy report to {paths.DeployReportPath} ...");
                await _fileSystemAccess.WriteFileAsync(paths.DeployReportPath, result.DeployReportContent);
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to write deploy report: {e.Message}");
                success = false;
            }

            return success;
        }

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {
            var success = await CreateScriptAsync(stateModel.Paths, stateModel.Configuration.CreateDocumentationWithScriptCreation);
            if (!success)
            {
                await _logger.LogAsync("ERROR: Script creation aborted.");
                stateModel.Result = false;
            }

            stateModel.CurrentState = StateModelState.TriedToCreateDeploymentFiles;
        }
    }
}