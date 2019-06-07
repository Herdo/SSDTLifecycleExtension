using System;
using System.Threading.Tasks;
using SSDTLifecycleExtension.Shared.Models;

namespace SSDTLifecycleExtension.Services
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Annotations;
    using Shared.Contracts;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Enums;
    using Shared.Contracts.Factories;
    using Shared.Contracts.Services;

    [UsedImplicitly]
    public class ScriptCreationService : IScriptCreationService
    {
        private readonly IVersionService _versionService;
        private readonly ISqlProjectService _sqlProjectService;
        private readonly IScriptModifierFactory _scriptModifierFactory;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;

        private bool _isCreating;

        public ScriptCreationService(IVersionService versionService,
                                     ISqlProjectService sqlProjectService,
                                     IScriptModifierFactory scriptModifierFactory,
                                     IVisualStudioAccess visualStudioAccess,
                                     IFileSystemAccess fileSystemAccess)
        {
            _versionService = versionService;
            _sqlProjectService = sqlProjectService;
            _scriptModifierFactory = scriptModifierFactory;
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

        private async Task<ScriptCreationVariables> CreateVariablesAsync(SqlProject project,
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
            var finalPreviousVersion = Version.Parse(previousVersionString);
            var finalNewVersion = createLatest
                                      ? new Version(finalPreviousVersion.Major + 1, int.MaxValue, int.MaxValue, int.MaxValue)
                                      : Version.Parse(newVersionString);

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

            return new ScriptCreationVariables(pi.SqlTargetName,
                                               projectPath,
                                               binaryDirectory,
                                               profilePath,
                                               newVersionDirectory,
                                               newVersionPath,
                                               previousVersionPath,
                                               outputPath,
                                               documentationPath,
                                               finalPreviousVersion,
                                               finalNewVersion);
        }

        private async Task<string> DetermineSqlPackagePathAsync(ConfigurationModel configuration)
        {
            // Determine SqlPackage.exe path
            if (configuration.SqlPackagePath != ConfigurationModel.SqlPackageSpecialKeyword)
                return configuration.SqlPackagePath;

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Searching latest SqlPackage.exe ...");
            var sqlPackageExecutables = _fileSystemAccess.SearchForFiles(Environment.SpecialFolder.ProgramFilesX86, "Microsoft SQL Server", "SqlPackage.exe");
            if (sqlPackageExecutables.Error != null)
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"ERROR: Failed to find any SqlPackage.exe: {sqlPackageExecutables.Error}");
                return null;
            }

            if (sqlPackageExecutables.Result.Length > 0)
                return sqlPackageExecutables.Result.OrderByDescending(m => m).First();

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("ERROR: Failed to find latest SqlPackage.exe. Please specify an absolute path to the SqlPackage.exe to use.");
            return null;
        }

        private async Task<bool> VerifyVariablesAsync(ScriptCreationVariables variables,
                                                      string sqlPackagePath)
        {
            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Verifying variables ...");
            if (!_fileSystemAccess.CheckIfFileExists(variables.ProjectPath))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("ERROR: Failed to find project file.");
                return false;
            }

            if (!_fileSystemAccess.CheckIfFileExists(variables.ProfilePath))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("ERROR: Failed to find publish profile.");
                return false;
            }

            if (!_fileSystemAccess.CheckIfFileExists(sqlPackagePath))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("ERROR: Failed to find SqlPackage.exe.");
                return false;
            }

            return true;
        }

        private async Task BuildAsync(SqlProject project,
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
            if (directoryCreationError != null)
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("ERROR: Failed to ensure the target directory exists.");
                return false;
            }

            var copyFilesError = _fileSystemAccess.CopyFiles(variables.BinaryDirectory, variables.SourceDirectory, "*.dacpac");
            if (copyFilesError == null)
                return true;

            await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"ERROR: Failed to copy files to the target directory: {copyFilesError}");
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

        private async Task<bool> ModifyCreatedScriptAsync(ConfigurationModel configuration,
                                                          ScriptCreationVariables variables,
                                                          CancellationToken cancellationToken)
        {
            var modifiers = GetScriptModifiers(configuration);
            if (!modifiers.Any())
                return true;

            var scriptContent = await _fileSystemAccess.ReadFileAsync(variables.DeployScriptPath);

            foreach (var m in modifiers.OrderBy(m => m.Key))
            {
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"Modifying script: {m.Key}");

                scriptContent = m.Value.Modify(scriptContent,
                                               configuration,
                                               variables);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;
            }

            await _fileSystemAccess.WriteFileAsync(variables.DeployScriptPath, scriptContent);

            return true;
        }

        private IReadOnlyDictionary<ScriptModifier, IScriptModifier> GetScriptModifiers(ConfigurationModel configuration)
        {
            var result = new Dictionary<ScriptModifier, IScriptModifier>();

            if (!string.IsNullOrWhiteSpace(configuration.CustomHeader))
                result[ScriptModifier.AddCustomHeader] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomHeader);

            if (!string.IsNullOrWhiteSpace(configuration.CustomFooter))
                result[ScriptModifier.AddCustomFooter] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomFooter);

            if (configuration.TrackDacpacVersion)
                result[ScriptModifier.TrackDacpacVersion] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.TrackDacpacVersion);

            return result;
        }

        public event EventHandler IsCreatingChanged;

        private bool IsCreating
        {
            get => _isCreating;
            set
            {
                if (value == _isCreating) return;
                _isCreating = value;
                IsCreatingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        bool IScriptCreationService.IsCreating => IsCreating;

        async Task IScriptCreationService.CreateAsync(SqlProject project,
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
            if (IsCreating)
                throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

            IsCreating = true;
            try
            {
                await _visualStudioAccess.StartLongRunningTaskIndicatorAsync();
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
                // Wait 1 second after creating the script to get any messages from the standard output before continuing with the script creation.
                await Task.Delay(1000, cancellationToken);

                if (!success)
                {
                    sw.Stop();
                    await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"ERROR: Script creation aborted after {sw.ElapsedMilliseconds / 1000} seconds.");
                    return;
                }

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                // Modify the script
                if (!await ModifyCreatedScriptAsync(configuration, variables, cancellationToken))
                    return;

                // No check for the cancellation token after the last action.
                // Completion
                sw.Stop();
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"========== Script creation finished after {sw.ElapsedMilliseconds / 1000} seconds. ==========");
            }
            catch (Exception e)
            {
                try
                {
                    await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync($"ERROR: Script creation failed: {e.Message}");
                }
                catch
                {
                    // ignored
                }
            }
            finally
            {
                try
                {
                    await _visualStudioAccess.StopLongRunningTaskIndicatorAsync();
                }
                catch
                {
                    // ignored
                }

                IsCreating = false;
            }
        }
    }
}