using System;
using System.Threading.Tasks;
using EnvDTE;
using SSDTLifecycleExtension.Shared.Models;

namespace SSDTLifecycleExtension.Services
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Annotations;
    using DataAccess;

    [UsedImplicitly]
    public class ScriptCreationService : IScriptCreationService
    {
        private readonly IVersionService _versionService;
        private readonly ISqlProjectService _sqlProjectService;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;

        private bool _isCreating;

        public ScriptCreationService(IVersionService versionService,
                                     ISqlProjectService sqlProjectService,
                                     IVisualStudioAccess visualStudioAccess,
                                     IFileSystemAccess fileSystemAccess)
        {
            _versionService = versionService;
            _sqlProjectService = sqlProjectService;
            _visualStudioAccess = visualStudioAccess;
            _fileSystemAccess = fileSystemAccess;
        }

        private async Task<bool> ShouldCancelAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Creation was canceled by the user.");
            return true;
        }

        private async Task<ScriptCreationVariables> CreateVariablesAsync(Project project,
                                                                         ConfigurationModel configuration,
                                                                         Version previousVersion,
                                                                         Version newVersion)
        {
            var projectPath = project.FullName;
            var projectDirectory = Path.GetDirectoryName(projectPath);
            if (projectDirectory == null)
                throw new InvalidOperationException("Cannot get project directory.");

            // DACPAC data from *.sqlproj
            var pi = await _sqlProjectService.GetSqlProjectInformationAsync(projectPath);
            var binaryDirectory = Path.Combine(projectDirectory, pi.OutputPath);

            // Versions
            var createLatest = newVersion == null;
            var previousVersionString = _versionService.DetermineFinalVersion(previousVersion, configuration);
            var newVersionString = createLatest ? "latest" : _versionService.DetermineFinalVersion(newVersion, configuration);

            // DACPAC paths
            var profilePath = Path.Combine(projectDirectory, configuration.PublishProfilePath);
            var artifactsPath = Path.Combine(projectDirectory, configuration.ArtifactsPath);
            var previousVersionDirectory = Path.Combine(artifactsPath, previousVersionString);
            var previousVersionPath = Path.Combine(previousVersionDirectory, $"{pi.SqlTargetName}.dacpac");
            var newVersionDirectory = Path.Combine(artifactsPath, newVersionString);
            var newVersionPath = Path.Combine(newVersionDirectory, $"{pi.SqlTargetName}.dacpac");
            var outputPath = Path.Combine(newVersionDirectory, $"{pi.SqlTargetName}_{previousVersionString}_{newVersionString}.sql");
            var documentationPath = configuration.CreateDocumentationWithScriptCreation
                ? Path.Combine(newVersionDirectory, $"{pi.SqlTargetName}_{previousVersionString}_{newVersionString}.xml")
                : null;

            return new ScriptCreationVariables(projectPath,
                                               binaryDirectory,
                                               profilePath,
                                               newVersionDirectory,
                                               newVersionPath,
                                               previousVersionPath,
                                               outputPath,
                                               documentationPath);
        }

        private async Task<string> DetermineSqlPackagePathAsync(ConfigurationModel configuration)
        {
            // Determine SqlPackage.exe path
            if (configuration.SqlPackagePath != ConfigurationModel.SqlPackageSpecialKeyword)
                return configuration.SqlPackagePath;

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Searching latest SqlPackage.exe ...");
            var sqlPackageExecutables = _fileSystemAccess.SearchForFiles(Environment.SpecialFolder.ProgramFilesX86, "Microsoft SQL Server", "SqlPackage.exe");
            if (sqlPackageExecutables.Length > 0)
                return sqlPackageExecutables.OrderByDescending(m => m).First();

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Failed to find latest SqlPackage.exe. Please specify an absolute path to the SqlPackage.exe to use.");
            return null;
        }

        private async Task<bool> VerifyVariablesAsync(ScriptCreationVariables variables,
                                                      string sqlPackagePath)
        {
            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Verifying variables ...");
            if (!_fileSystemAccess.CheckIfFileExists(variables.ProjectPath))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Failed to find project file.");
                return false;
            }

            if (!_fileSystemAccess.CheckIfFileExists(variables.ProfilePath))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Failed to find publish profile.");
                return false;
            }

            if (!_fileSystemAccess.CheckIfFileExists(sqlPackagePath))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Failed to find SqlPackage.exe.");
                return false;
            }

            return true;
        }

        private async Task BuildAsync(Project project,
                                      ConfigurationModel configuration)
        {
            if (configuration.BuildBeforeScriptCreation)
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Building project ...");
                _visualStudioAccess.BuildProject(project);
            }
        }

        private async Task<bool> CopyOutputAsync(ScriptCreationVariables variables)
        {
            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Copying files to target directory ...");
            var directoryCreationError = _fileSystemAccess.EnsureDirectoryExists(variables.SourceDirectory);
            if (directoryCreationError == null)
            {
                var copyFilesError = _fileSystemAccess.CopyFiles(variables.BinaryDirectory, variables.SourceDirectory, "*.dacpac");
                if (copyFilesError == null)
                    return true;

                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"ERROR: Failed to copy files to the target directory: {copyFilesError}");
                return false;
            }

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("ERROR: Failed to ensure the target directory exists.");
            return false;
        }

        private async Task<bool> CreateScriptAsync(ScriptCreationVariables variables,
                                                   string sqlPackagePath,
                                                   CancellationToken cancellationToken)
        {
            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Creating script ...");
            var sqlPackageArguments = new StringBuilder();
            sqlPackageArguments.Append("/Action:Script ");
            sqlPackageArguments.Append($"/Profile:\"{variables.ProfilePath}\" ");
            sqlPackageArguments.Append($"/SourceFile:\"{variables.SourceFile}\" "); // new version
            sqlPackageArguments.Append($"/TargetFile:\"{variables.TargetFile}\" "); // previous version from artifacts directory
            sqlPackageArguments.Append($"/DeployScriptPath:\"{variables.DeployScriptPath}\" ");
            if (variables.CreateDocumentation)
                sqlPackageArguments.Append($"/DeployReportPath:\"{variables.DeployReportPath}\" ");
            var hasErrors = false;
            var processStartError = await _fileSystemAccess.StartProcessAndWaitAsync(sqlPackagePath,
                                                                                     sqlPackageArguments.ToString(),
                                                                                     async s =>
                                                                                     {
                                                                                         if (!string.IsNullOrWhiteSpace(s))
                                                                                             await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"SqlPackage.exe INFO: {s}");
                                                                                     },
                                                                                     async s =>
                                                                                     {
                                                                                         if (!string.IsNullOrWhiteSpace(s))
                                                                                         {
                                                                                             hasErrors = true;
                                                                                             await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"SqlPackage.exe ERROR: {s}");
                                                                                         }
                                                                                     },
                                                                                     cancellationToken);
            if (processStartError == null)
                return !hasErrors;

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"ERROR: Failed to start the SqlPackage.exe process: {processStartError}");
            return false;
        }

        bool IScriptCreationService.IsCreating => _isCreating;

        async Task IScriptCreationService.CreateAsync(Project project,
                                                      ConfigurationModel configuration,
                                                      Version previousVersion,
                                                      Version newVersion,
                                                      CancellationToken cancellationToken)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (previousVersion == null)
                throw new ArgumentNullException(nameof(previousVersion));
            if (_isCreating)
                throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

            _isCreating = true;
            try
            {
                await _visualStudioAccess.ClearSSDTLifecycleOutputAsync();
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Initializing script creation ...");
                var sw = new Stopwatch();
                sw.Start();
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                // No check for the cancellation token before the first action.
                var variables = await CreateVariablesAsync(project, configuration, previousVersion, newVersion);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                var sqlPackagePath = await DetermineSqlPackagePathAsync(configuration);
                if (sqlPackagePath == null)
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                if (!await VerifyVariablesAsync(variables, sqlPackagePath))
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                await BuildAsync(project, configuration);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                if (!await CopyOutputAsync(variables))
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                var success = await CreateScriptAsync(variables, sqlPackagePath, cancellationToken);
                if (!success)
                {
                    sw.Stop();
                    await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"Script creation aborted after {sw.ElapsedMilliseconds / 1000} seconds.");
                    return;
                }

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                // Modify the script


                // No check for the cancellation token after the last action.
                // Completion
                sw.Stop();
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"Script creation finished after {sw.ElapsedMilliseconds / 1000} seconds.");
            }
            catch (Exception e)
            {
                try
                {
                    await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"Script creation failed: {e.Message}");
                }
                catch
                { }
            }
            finally
            {
                _isCreating = false;
            }
        }

        private struct ScriptCreationVariables
        {
            public string ProjectPath { get; }
            public string BinaryDirectory { get; }
            public string ProfilePath { get; }
            public string SourceDirectory { get; }
            public string SourceFile { get; }
            public string TargetFile { get; }
            public string DeployScriptPath { get; }
            public string DeployReportPath { get; }
            public bool CreateDocumentation { get; }

            public ScriptCreationVariables(string projectPath,
                                           string binaryDirectory,
                                           string profilePath,
                                           string sourceDirectory,
                                           string sourceFile,
                                           string targetFile,
                                           string deployScriptPath,
                                           string deployReportPath)
            {
                ProjectPath = projectPath;
                BinaryDirectory = binaryDirectory;
                ProfilePath = profilePath;
                SourceDirectory = sourceDirectory;
                SourceFile = sourceFile;
                TargetFile = targetFile;
                DeployScriptPath = deployScriptPath;
                DeployReportPath = deployReportPath;
                CreateDocumentation = deployReportPath != null;
            }
        }
    }
}