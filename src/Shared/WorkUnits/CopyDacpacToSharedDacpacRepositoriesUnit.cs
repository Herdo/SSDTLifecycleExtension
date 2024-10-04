namespace SSDTLifecycleExtension.Shared.WorkUnits;

public class CopyDacpacToSharedDacpacRepositoriesUnit(IFileSystemAccess _fileSystemAccess,
                                                      ILogger _logger)
    : IWorkUnit<ScaffoldingStateModel>,
    IWorkUnit<ScriptCreationStateModel>
{
    private async Task TryCopyInternal(IStateModel stateModel,
        string projectDirectory,
        string newDacpacPath,
        string? sharedDacpacRepositoryPaths)
    {
        if (string.IsNullOrWhiteSpace(sharedDacpacRepositoryPaths))
        {
            stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
            return;
        }

        await _logger.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ...");
        string fileName;
        string[] paths;

        try
        {
            fileName = Path.GetFileName(newDacpacPath);
            paths = sharedDacpacRepositoryPaths!.Split([';'], StringSplitOptions.RemoveEmptyEntries);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync($"Failed to copy DACPAC to shared DACPAC repositories: {e.Message}");
            stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
            stateModel.Result = false;
            return;
        }
        foreach (var path in paths)
        {
            var targetDirectory = path;
            try
            {
                if (!Path.IsPathRooted(targetDirectory))
                {
                    if (!targetDirectory.StartsWith("\\")
                     && !targetDirectory.StartsWith(".\\")
                     && !targetDirectory.StartsWith("..\\"))
                        targetDirectory = "\\" + targetDirectory;
                    targetDirectory = Path.GetFullPath(projectDirectory + targetDirectory);
                    await _logger.LogTraceAsync($"Resolved relative path '{path}' to absolute path '{targetDirectory}'.");
                }

                var directoryError = _fileSystemAccess.EnsureDirectoryExists(targetDirectory!);
                if (directoryError is not null)
                {
                    await _logger.LogErrorAsync($"Failed to ensure that the directory '{targetDirectory}' exists: {directoryError}");
                    stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                    stateModel.Result = false;
                    continue;
                }

                var targetFile = Path.Combine(targetDirectory, fileName);
                await _logger.LogTraceAsync($"Copying DACPAC to '{targetFile}' ...");
                var copyError = _fileSystemAccess.CopyFile(newDacpacPath, targetFile);
                stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                if (copyError == null)
                    continue;

                await _logger.LogErrorAsync($"Failed to copy DACPAC to shared DACPAC repository at '{targetDirectory}': {copyError}");
                stateModel.Result = false;
            }
            catch (Exception e)
            {
                await _logger.LogErrorAsync($"Failed to copy DACPAC to shared DACPAC repository at '{targetDirectory}': {e.Message}");
                stateModel.CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository;
                stateModel.Result = false;
            }
        }
    }

    Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.NewDacpacPath);

        return TryCopyInternal(stateModel,
            stateModel.Paths.Directories.ProjectDirectory,
            stateModel.Paths.DeploySources.NewDacpacPath,
            stateModel.Configuration.SharedDacpacRepositoryPaths);
    }

    Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
        CancellationToken cancellationToken)
    {
        Guard.IsNotNullOrWhiteSpace(stateModel.Paths?.DeploySources.NewDacpacPath);

        return TryCopyInternal(stateModel,
            stateModel.Paths.Directories.ProjectDirectory,
            stateModel.Paths.DeploySources.NewDacpacPath,
            stateModel.Configuration.SharedDacpacRepositoryPaths);
    }
}