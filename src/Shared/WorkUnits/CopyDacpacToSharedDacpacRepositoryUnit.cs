namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using Contracts.Models;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class CopyDacpacToSharedDacpacRepositoryUnit : IWorkUnit<ScaffoldingStateModel>,
                                                          IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
        [NotNull] private readonly ILogger _logger;

        public CopyDacpacToSharedDacpacRepositoryUnit([NotNull] IFileSystemAccess fileSystemAccess,
                                                      [NotNull] ILogger logger)
        {
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task TryCopyInternal(IStateModel stateModel,
                                           string newDacpacPath,
                                           string sharedDacpacRepositoryPath)
        {
            if (string.IsNullOrWhiteSpace(sharedDacpacRepositoryPath))
            {
                stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                return;
            }

            await _logger.LogAsync("Copying DACPAC to shared DACPAC repository ...");
            try
            {
                var fileName = Path.GetFileName(newDacpacPath);
                if (fileName == null)
                {
                    await _logger.LogAsync($"ERROR: Cannot get file name from path '{newDacpacPath}'.");
                    stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                    stateModel.Result = false;
                    return;
                }
                var targetFile = Path.Combine(sharedDacpacRepositoryPath, fileName);
                var directoryError = _fileSystemAccess.EnsureDirectoryExists(sharedDacpacRepositoryPath);
                if (directoryError != null)
                {
                    await _logger.LogAsync($"ERROR: Failed to ensure that the directory '{sharedDacpacRepositoryPath}' exists.");
                    stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                    stateModel.Result = false;
                }

                var copyError = _fileSystemAccess.CopyFile(newDacpacPath, targetFile);
                stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                if (copyError == null)
                    return;

                await _logger.LogAsync($"ERROR: Failed to copy DACPAC to shared DACPAC repository: {copyError}");
                stateModel.Result = false;
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to copy DACPAC to shared DACPAC repository: {e.Message}");
                stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                stateModel.Result = false;
            }
        }

        Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                   CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return TryCopyInternal(stateModel,
                                   stateModel.Paths.DeploySources.NewDacpacPath,
                                   stateModel.Configuration.SharedDacpacRepositoryPath);
        }

        Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                      CancellationToken cancellationToken)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            return TryCopyInternal(stateModel,
                                   stateModel.Paths.DeploySources.NewDacpacPath,
                                   stateModel.Configuration.SharedDacpacRepositoryPath);
        }
    }
}