namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
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
        private readonly IVersionService _versionService;
        private readonly IScriptModifierFactory _scriptModifierFactory;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;
        private readonly IDacAccess _dacAccess;
        private readonly ILogger _logger;

        private bool _isCreating;

        public ScriptCreationService(ISqlProjectService sqlProjectService,
                                     IBuildService buildService,
                                     IVersionService versionService,
                                     IScriptModifierFactory scriptModifierFactory,
                                     IVisualStudioAccess visualStudioAccess,
                                     IFileSystemAccess fileSystemAccess,
                                     IDacAccess dacAccess,
                                     ILogger logger)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _scriptModifierFactory = scriptModifierFactory ?? throw new ArgumentNullException(nameof(scriptModifierFactory));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _dacAccess = dacAccess ?? throw new ArgumentNullException(nameof(dacAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> ShouldCancelAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            await _logger.LogAsync("Creation was canceled by the user.");
            return true;
        }

        private async Task<bool> VerifyPathsAsync(PathCollection paths)
        {
            await _logger.LogAsync("Verifying paths ...");

            if (!_fileSystemAccess.CheckIfFileExists(paths.PublishProfilePath))
            {
                await _logger.LogAsync("ERROR: Failed to find publish profile.");
                return false;
            }

            return true;
        }

        private async Task<bool> CreateScriptAsync(PathCollection paths,
                                                   bool createDocumentation)
        {
            await _logger.LogAsync("Creating diff files ...");
            var result = await _dacAccess.CreateDeployFilesAsync(paths.PreviousDacpacPath,
                                                                 paths.NewDacpacPath,
                                                                 paths.PublishProfilePath,
                                                                 true,
                                                                 createDocumentation);

            if (result.Errors != null)
            {
                await _logger.LogAsync("ERROR: Failed to create script:");
                foreach (var s in result.Errors)
                    await _logger.LogAsync(s);

                return false;
            }

            var success = true;
            try
            {
                await _logger.LogAsync($"Writing deploy script to {paths.DeployScriptPath} ...");
                await _fileSystemAccess.WriteFileAsync(paths.DeployScriptPath, result.DeployScriptContent);
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to write deploy script: {e.Message}");
                success = false;
            }

            if (!createDocumentation)
                return success;

            try
            {
                await _logger.LogAsync($"Writing deploy report to {paths.DeployReportPath} ...");
                await _fileSystemAccess.WriteFileAsync(paths.DeployReportPath, result.DeployReportContent);
            }
            catch (Exception e)
            {
                await _logger.LogAsync($"ERROR: Failed to write deploy report: {e.Message}");
                success = false;
            }

            return success;
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
                                               configuration,
                                               paths);

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

            if (configuration.CommentOutUnnamedDefaultConstraintDrops)
                result[ScriptModifier.CommentOutUnnamedDefaultConstraintDrops] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops);

            if (configuration.ReplaceUnnamedDefaultConstraintDrops)
                result[ScriptModifier.ReplaceUnnamedDefaultConstraintDrops] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops);

            if (!string.IsNullOrWhiteSpace(configuration.CustomHeader))
                result[ScriptModifier.AddCustomHeader] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomHeader);

            if (!string.IsNullOrWhiteSpace(configuration.CustomFooter))
                result[ScriptModifier.AddCustomFooter] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomFooter);

            if (configuration.TrackDacpacVersion)
                result[ScriptModifier.TrackDacpacVersion] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.TrackDacpacVersion);

            return result;
        }

        private async Task<bool> CreateInternalAsync(SqlProject project,
                                               ConfigurationModel configuration,
                                               Version previousVersion,
                                               bool latest,
                                               CancellationToken cancellationToken)
        {
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
                    return false;

                if (!await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(project))
                    return false;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                // Check DacVersion against base version, if not running latest creation
                if (!latest)
                {
                    var formattedTargetVersion = Version.Parse(_versionService.DetermineFinalVersion(project.ProjectProperties.DacVersion, configuration));
                    if (formattedTargetVersion <= previousVersion)
                    {
                        await _logger.LogAsync($"ERROR: DacVersion of SQL project ({formattedTargetVersion}) is equal to or smaller than the previous version ({previousVersion}).");
                        _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
                        return false;
                    }
                }

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                // Create paths required for script creation
                var paths = await _sqlProjectService.TryLoadPathsAsync(project, configuration, previousVersion, latest);
                if (paths == null)
                    return false;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                if (!await VerifyPathsAsync(paths))
                    return false;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                if (configuration.BuildBeforeScriptCreation)
                    await _buildService.BuildProjectAsync(project);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                if (!await _buildService.CopyBuildResultAsync(project, paths.NewDacpacDirectory))
                    return false;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                var success = await CreateScriptAsync(paths, configuration.CreateDocumentationWithScriptCreation);
                if (!success)
                {
                    sw.Stop();
                    await _logger.LogAsync($"ERROR: Script creation aborted after {sw.ElapsedMilliseconds / 1000} seconds.");
                    return false;
                }

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                // Modify the script
                if (!await ModifyCreatedScriptAsync(project, configuration, paths, cancellationToken))
                    return false;

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

            return true;
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

        Task<bool> IScriptCreationService.CreateAsync(SqlProject project,
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

            return CreateInternalAsync(project, configuration, previousVersion, latest, cancellationToken);
        }
    }
}