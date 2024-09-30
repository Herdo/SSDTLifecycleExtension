namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class CopyDacpacToSharedDacpacRepositoryUnit(IFileSystemAccess _fileSystemAccess,
                                                    ILogger _logger)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private async Task TryCopyInternal(IStateModel stateModel,
        string newDacpacPath,
        string? sharedDacpacRepositoryPath)
    {
        if (string.IsNullOrWhiteSpace(sharedDacpacRepositoryPath))
        {
            stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
            return;
        }

        await _logger.LogInfoAsync("Copying DACPAC to shared DACPAC repository ...");
        try
        {
            var fileName = Path.GetFileName(newDacpacPath);
            var targetFile = Path.Combine(sharedDacpacRepositoryPath, fileName);
            var directoryError = _fileSystemAccess.EnsureDirectoryExists(sharedDacpacRepositoryPath!);
            if (directoryError is not null)
            {
                await _logger.LogErrorAsync($"Failed to ensure that the directory '{sharedDacpacRepositoryPath}' exists: {directoryError}");
                stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                stateModel.Result = false;
                return;
            }

            var copyError = _fileSystemAccess.CopyFile(newDacpacPath, targetFile);
            stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
            if (copyError == null)
                return;

            await _logger.LogErrorAsync($"Failed to copy DACPAC to shared DACPAC repository: {copyError}");
            stateModel.Result = false;
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync($"Failed to copy DACPAC to shared DACPAC repository: {e.Message}");
            stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
            stateModel.Result = false;
        }
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.NewDacpacPath);

        return TryCopyInternal(stateModel,
            stateModel.Paths.DeploySources.NewDacpacPath,
            stateModel.Configuration.SharedDacpacRepositoryPath);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.NewDacpacPath);

        return TryCopyInternal(stateModel,
            stateModel.Paths.DeploySources.NewDacpacPath,
            stateModel.Configuration.SharedDacpacRepositoryPath);
    }
}