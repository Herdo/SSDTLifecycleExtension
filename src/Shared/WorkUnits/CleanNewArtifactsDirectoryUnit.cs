namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class CleanNewArtifactsDirectoryUnit(IFileSystemAccess _fileSystemAccess,
                                            ILogger _logger)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private async Task CleanArtifactsDirectoryInternal(IStateModel stateModel,
                                                       PathCollection paths)
    {
        await _logger.LogInfoAsync("Cleaning artifacts directory ...");
        // Even if this operation fails, there's no reason to make the whole process fail.
        // Therefore this will not set the stateModel.Result property.
        _fileSystemAccess.TryToCleanDirectory(paths.Directories.NewArtifactsDirectory);

        stateModel.CurrentState = StateModelState.TriedToCleanArtifactsDirectory;
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.Directories.NewArtifactsDirectory);

        return CleanArtifactsDirectoryInternal(stateModel,
            stateModel.Paths);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.Directories.NewArtifactsDirectory);

        return CleanArtifactsDirectoryInternal(stateModel,
            stateModel.Paths);
    }
}