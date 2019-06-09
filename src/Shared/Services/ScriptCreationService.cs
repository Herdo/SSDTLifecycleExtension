namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using Contracts.Factories;
    using Contracts.Services;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ScriptCreationService : IScriptCreationService
    {
        private readonly ISqlProjectService _sqlProjectService;
        private readonly IBuildService _buildService;
        private readonly IScriptModifierFactory _scriptModifierFactory;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;
        private readonly ILogger _logger;

        private bool _isCreating;

        public ScriptCreationService(ISqlProjectService sqlProjectService,
                                     IBuildService buildService,
                                     IScriptModifierFactory scriptModifierFactory,
                                     IVisualStudioAccess visualStudioAccess,
                                     IFileSystemAccess fileSystemAccess,
                                     ILogger logger)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
            _scriptModifierFactory = scriptModifierFactory ?? throw new ArgumentNullException(nameof(scriptModifierFactory));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> ShouldCancelAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            await _logger.LogAsync("Creation was canceled by the user.");
            return true;
        }

        private async Task<string> DetermineSqlPackagePathAsync(ConfigurationModel configuration)
        {
            // Determine SqlPackage.exe path
            if (configuration.SqlPackagePath != ConfigurationModel.SqlPackageSpecialKeyword)
                return configuration.SqlPackagePath;

            await _logger.LogAsync("Searching latest SqlPackage.exe ...");
            var sqlPackageExecutables = _fileSystemAccess.SearchForFiles(Environment.SpecialFolder.ProgramFilesX86, "Microsoft SQL Server", "SqlPackage.exe");
            if (sqlPackageExecutables.Error != null)
            {
                await _logger.LogAsync($"ERROR: Failed to find any SqlPackage.exe: {sqlPackageExecutables.Error}");
                return null;
            }

            if (sqlPackageExecutables.Result.Length > 0)
                return sqlPackageExecutables.Result.OrderByDescending(m => m).First();

            await _logger.LogAsync("ERROR: Failed to find latest SqlPackage.exe. Please specify an absolute path to the SqlPackage.exe to use.");
            return null;
        }

        private async Task<bool> VerifyPathsAsync(PathCollection paths,
                                                  string sqlPackagePath)
        {
            await _logger.LogAsync("Verifying paths ...");

            if (!_fileSystemAccess.CheckIfFileExists(paths.PublishProfilePath))
            {
                await _logger.LogAsync("ERROR: Failed to find publish profile.");
                return false;
            }

            if (!_fileSystemAccess.CheckIfFileExists(sqlPackagePath))
            {
                await _logger.LogAsync("ERROR: Failed to find SqlPackage.exe.");
                return false;
            }

            return true;
        }

        private async Task<bool> CreateScriptAsync(PathCollection paths,
                                                   bool createDocumentation,
                                                   string sqlPackagePath,
                                                   CancellationToken cancellationToken)
        {
            await _logger.LogAsync("Creating script ...");
            var sqlPackageArguments = new StringBuilder();
            sqlPackageArguments.Append("/Action:Script ");
            sqlPackageArguments.Append($"/Profile:\"{paths.PublishProfilePath}\" ");
            sqlPackageArguments.Append($"/SourceFile:\"{paths.NewDacpacPath}\" "); // new version
            sqlPackageArguments.Append($"/TargetFile:\"{paths.PreviousDacpacPath}\" "); // previous version from artifacts directory
            sqlPackageArguments.Append($"/DeployScriptPath:\"{paths.DeployScriptPath}\" ");
            if (createDocumentation)
                sqlPackageArguments.Append($"/DeployReportPath:\"{paths.DeployReportPath}\" ");
            var hasErrors = false;
            var processStartError = await _fileSystemAccess.StartProcessAndWaitAsync(sqlPackagePath,
                                                                                     sqlPackageArguments.ToString(),
                                                                                     async s =>
                                                                                     {
                                                                                         if (!string.IsNullOrWhiteSpace(s))
                                                                                             await _logger.LogAsync($"SqlPackage.exe INFO: {s}");
                                                                                     },
                                                                                     async s =>
                                                                                     {
                                                                                         if (!string.IsNullOrWhiteSpace(s))
                                                                                         {
                                                                                             hasErrors = true;
                                                                                             await _logger.LogAsync($"SqlPackage.exe ERROR: {s}");
                                                                                         }
                                                                                     },
                                                                                     cancellationToken);
            if (processStartError == null)
                return !hasErrors;

            await _logger.LogAsync($"ERROR: Failed to start the SqlPackage.exe process: {processStartError}");
            return false;
        }

        private async Task<bool> ModifyCreatedScriptAsync(SqlProject project,
                                                          ConfigurationModel configuration,
                                                          PathCollection paths,
                                                          CancellationToken cancellationToken)
        {
            var modifiers = GetScriptModifiers(configuration);
            if (!modifiers.Any())
                return true;

            var scriptContent = await _fileSystemAccess.ReadFileAsync(paths.DeployScriptPath);

            foreach (var m in modifiers.OrderBy(m => m.Key))
            {
                await _logger.LogAsync($"Modifying script: {m.Key}");

                scriptContent = m.Value.Modify(scriptContent,
                                               project,
                                               configuration);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;
            }

            await _fileSystemAccess.WriteFileAsync(paths.DeployScriptPath, scriptContent);

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
                                                      bool latest,
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
                await _logger.LogAsync("Initializing script creation ...");
                var sw = new Stopwatch();
                sw.Start();

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                if(!await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(project))
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                // Create paths required for script creation
                var paths = await _sqlProjectService.TryLoadPathsAsync(project, configuration, previousVersion, latest);
                if (paths == null)
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                var sqlPackagePath = await DetermineSqlPackagePathAsync(configuration);
                if (sqlPackagePath == null)
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                if (!await VerifyPathsAsync(paths, sqlPackagePath))
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                if (configuration.BuildBeforeScriptCreation)
                    await _buildService.BuildProjectAsync(project);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                if (!await _buildService.CopyBuildResultAsync(project, paths.NewDacpacDirectory))
                    return;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                var success = await CreateScriptAsync(paths, configuration.CreateDocumentationWithScriptCreation, sqlPackagePath, cancellationToken);
                // Wait 1 second after creating the script to get any messages from the standard output before continuing with the script creation.
                await Task.Delay(1000, cancellationToken);

                if (!success)
                {
                    sw.Stop();
                    await _logger.LogAsync($"ERROR: Script creation aborted after {sw.ElapsedMilliseconds / 1000} seconds.");
                    return;
                }

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                // Modify the script
                if (!await ModifyCreatedScriptAsync(project, configuration, paths, cancellationToken))
                    return;

                // No check for the cancellation token after the last action.
                // Completion
                sw.Stop();
                await _logger.LogAsync($"========== Script creation finished after {sw.ElapsedMilliseconds} milliseconds. ==========");
            }
            catch (Exception e)
            {
                try
                {
                    await _logger.LogAsync($"ERROR: Script creation failed: {e.Message}");
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