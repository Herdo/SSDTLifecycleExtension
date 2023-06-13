namespace SSDTLifecycleExtension.Shared.WorkUnits;

[UsedImplicitly]
public class DeleteRefactorLogUnit : IWorkUnit<ScriptCreationStateModel>
{
    [NotNull] private readonly IFileSystemAccess _fileSystemAccess;
    [NotNull] private readonly IVisualStudioAccess _visualStudioAccess;
    [NotNull] private readonly ILogger _logger;

    public DeleteRefactorLogUnit([NotNull] IFileSystemAccess fileSystemAccess,
                                 [NotNull] IVisualStudioAccess visualStudioAccess,
                                 [NotNull] ILogger logger)
    {
        _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
        _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private async Task TryToDeleteRefactorLogInternal(IStateModel stateModel,
                                                      SqlProject project,
                                                      ConfigurationModel configuration,
                                                      PathCollection paths)
    {
        if (!configuration.DeleteRefactorlogAfterVersionedScriptGeneration)
        {
            stateModel.CurrentState = StateModelState.DeletedRefactorLog;
            return;
        }

        await _logger.LogInfoAsync("Deleting refactorlog files ...");
        var deletedFiles = _fileSystemAccess.TryToCleanDirectory(paths.Directories.ProjectDirectory, "*.refactorlog");
        if (deletedFiles.Length == 0)
            await _logger.LogTraceAsync("No files were deleted.");
        else
            foreach (var deletedFile in deletedFiles)
            {
                _visualStudioAccess.RemoveItemFromProjectRoot(project, deletedFile);
                await _logger.LogTraceAsync($"Deleted file {deletedFile} ...");
            }

        stateModel.CurrentState = StateModelState.DeletedRefactorLog;
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                  CancellationToken cancellationToken)
    {
        if (stateModel == null)
            throw new ArgumentNullException(nameof(stateModel));

        return TryToDeleteRefactorLogInternal(stateModel,
                                              stateModel.Project,
                                              stateModel.Configuration,
                                              stateModel.Paths);
    }
}