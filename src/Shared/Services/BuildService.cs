namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Threading.Tasks;
    using Contracts.DataAccess;
    using Contracts.Services;
    using Contracts;

    public class BuildService : IBuildService
    {
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;
        private readonly ILogger _logger;

        public BuildService(IVisualStudioAccess visualStudioAccess,
                            IFileSystemAccess fileSystemAccess,
                            ILogger logger)
        {
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        async Task IBuildService.BuildProjectAsync(SqlProject project)
        {
            await _logger.LogAsync("Building project ...");
            _visualStudioAccess.BuildProject(project);
        }

        async Task<bool> IBuildService.CopyBuildResultAsync(SqlProject project,
                                                            string targetDirectory)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (targetDirectory == null)
                throw new ArgumentNullException(nameof(targetDirectory));
            if (string.IsNullOrWhiteSpace(project.ProjectProperties.BinaryDirectory))
                throw new ArgumentException($"{nameof(SqlProjectProperties.BinaryDirectory)} must be filled.", nameof(project));

            await _logger.LogAsync("Copying files to target directory ...");
            var directoryCreationError = _fileSystemAccess.EnsureDirectoryExists(project.ProjectProperties.BinaryDirectory);
            if (directoryCreationError != null)
            {
                await _logger.LogAsync("ERROR: Failed to ensure the target directory exists.");
                return false;
            }

            var copyFilesError = _fileSystemAccess.CopyFiles(project.ProjectProperties.BinaryDirectory, targetDirectory, "*.dacpac");
            if (copyFilesError == null)
                return true;

            await _logger.LogAsync($"ERROR: Failed to copy files to the target directory: {copyFilesError}");
            return false;
        }
    }
}