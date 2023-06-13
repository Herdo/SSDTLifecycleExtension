namespace SSDTLifecycleExtension.Shared.WorkUnits;

[UsedImplicitly]
public class CleanNewArtifactsDirectoryUnit : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
    [NotNull] private readonly ILogger _logger;

    public CleanNewArtifactsDirectoryUnit([NotNull] IFileSystemAccess fileSystemAccess,
                                          [NotNull] ILogger logger)
    {
        _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

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
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return CleanArtifactsDirectoryInternal(stateModel, stateModel.Paths);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return CleanArtifactsDirectoryInternal(stateModel, stateModel.Paths);
    }
}