namespace SSDTLifecycleExtension.Shared.Services;

public class BuildService(IVisualStudioAccess _visualStudioAccess,
                          IFileSystemAccess _fileSystemAccess,
                          ILogger _logger)
    : IBuildService
{
    private async Task<bool> BuildProjectInternalAsync(SqlProject project)
    {
        await _logger.LogInfoAsync("Building project ...");
        try
        {
            _visualStudioAccess.BuildProject(project);
            return true;
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, $"Failed to build {project.FullName}");
            return false;
        }
    }

    private async Task<bool> CopyBuildResultInternalAsync(SqlProject project,
                                                          string targetDirectory)
    {
        await _logger.LogInfoAsync("Copying files to target directory ...");
        var directoryCreationError = _fileSystemAccess.EnsureDirectoryExists(targetDirectory);
        if (directoryCreationError != null)
        {
            await _logger.LogErrorAsync($"Failed to ensure the target directory exists: {directoryCreationError}");
            return false;
        }

        var copyFilesResult = _fileSystemAccess.CopyFiles(project.ProjectProperties.BinaryDirectory!, targetDirectory, "*.dacpac");
        foreach (var (source, target) in copyFilesResult.CopiedFiles)
            await _logger.LogTraceAsync($"Copied file \"{source}\" to \"{target}\" ...");

        if (copyFilesResult.Errors.Length == 0)
            return true;

        await _logger.LogErrorAsync("Failed to copy files to the target directory.");
        foreach (var (file, exception) in copyFilesResult.Errors)
            if (file == null)
                await _logger.LogErrorAsync(exception, "Failed to access the directory");
            else
                await _logger.LogErrorAsync(exception, $"Failed to copy file {file}");
        return false;
    }

    Task<bool> IBuildService.BuildProjectAsync(SqlProject project)
    {
        return BuildProjectInternalAsync(project);
    }

    Task<bool> IBuildService.CopyBuildResultAsync(SqlProject project,
        string targetDirectory)
    {
        if (string.IsNullOrWhiteSpace(project.ProjectProperties.BinaryDirectory))
            throw new ArgumentException($"{nameof(SqlProjectProperties.BinaryDirectory)} must be filled.", nameof(project));

        return CopyBuildResultInternalAsync(project, targetDirectory);
    }
}