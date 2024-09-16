namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class DeleteRefactorLogUnit(IFileSystemAccess _fileSystemAccess,
                                   IVisualStudioAccess _visualStudioAccess,
                                   ILogger _logger)
    : IWorkUnit<ScriptCreationStateModel>
{
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
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.Directories.ProjectDirectory);

        return TryToDeleteRefactorLogInternal(stateModel,
            stateModel.Project,
            stateModel.Configuration,
            stateModel.Paths);
    }
}