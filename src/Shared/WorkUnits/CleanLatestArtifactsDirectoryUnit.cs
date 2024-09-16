namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class CleanLatestArtifactsDirectoryUnit(IFileSystemAccess _fileSystemAccess,
                                               ILogger _logger)
    : IWorkUnit<ScriptCreationStateModel>
{
    private async Task CleanArtifactsDirectoryInternal(IStateModel stateModel,
        ConfigurationModel configuration,
        PathCollection paths)
    {
        if (!configuration.DeleteLatestAfterVersionedScriptGeneration)
        {
            stateModel.CurrentState = StateModelState.DeletedLatestArtifacts;
            return;
        }

        await _logger.LogInfoAsync("Cleaning latest artifacts directory ...");
        // Even if this operation fails, there's no reason to make the whole process fail.
        // Therefore this will not set the stateModel.Result property.
        _fileSystemAccess.TryToCleanDirectory(paths.Directories.LatestArtifactsDirectory);

        stateModel.CurrentState = StateModelState.DeletedLatestArtifacts;
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.Directories.LatestArtifactsDirectory);

        return CleanArtifactsDirectoryInternal(stateModel,
            stateModel.Configuration,
            stateModel.Paths);
    }
}