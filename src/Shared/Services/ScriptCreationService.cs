

namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Collections.Generic;
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
    public class ScriptCreationService : AsyncExecutorBase<ScriptCreationStateModel>, IScriptCreationService
    {
        private readonly ISqlProjectService _sqlProjectService;
        private readonly IBuildService _buildService;
        private readonly IVersionService _versionService;
        private readonly IScriptModifierFactory _scriptModifierFactory;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;
        private readonly IDacAccess _dacAccess;

        private bool _isCreating;

        public ScriptCreationService(ISqlProjectService sqlProjectService,
                                     IBuildService buildService,
                                     IVersionService versionService,
                                     IScriptModifierFactory scriptModifierFactory,
                                     IVisualStudioAccess visualStudioAccess,
                                     IFileSystemAccess fileSystemAccess,
                                     IDacAccess dacAccess,
                                     ILogger logger)
            : base(logger)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _scriptModifierFactory = scriptModifierFactory ?? throw new ArgumentNullException(nameof(scriptModifierFactory));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _dacAccess = dacAccess ?? throw new ArgumentNullException(nameof(dacAccess));
        }

        private async Task<bool> CreateScriptAsync(PathCollection paths,
                                                   bool createDocumentation)
        {
            await Logger.LogAsync("Creating diff files ...");
            var result = await _dacAccess.CreateDeployFilesAsync(paths.PreviousDacpacPath,
                                                                 paths.NewDacpacPath,
                                                                 paths.PublishProfilePath,
                                                                 true,
                                                                 createDocumentation);

            if (result.Errors != null)
            {
                await Logger.LogAsync("ERROR: Failed to create script:");
                foreach (var s in result.Errors)
                    await Logger.LogAsync(s);

                return false;
            }

            var success = true;
            try
            {
                await Logger.LogAsync($"Writing deploy script to {paths.DeployScriptPath} ...");
                await _fileSystemAccess.WriteFileAsync(paths.DeployScriptPath, result.DeployScriptContent);
            }
            catch (Exception e)
            {
                await Logger.LogAsync($"ERROR: Failed to write deploy script: {e.Message}");
                success = false;
            }

            if (!createDocumentation)
                return success;

            try
            {
                await Logger.LogAsync($"Writing deploy report to {paths.DeployReportPath} ...");
                await _fileSystemAccess.WriteFileAsync(paths.DeployReportPath, result.DeployReportContent);
            }
            catch (Exception e)
            {
                await Logger.LogAsync($"ERROR: Failed to write deploy report: {e.Message}");
                success = false;
            }

            return success;
        }

        private async Task ModifyCreatedScriptAsync(SqlProject project,
                                                    ConfigurationModel configuration,
                                                    PathCollection paths)
        {
            var modifiers = GetScriptModifiers(configuration);
            if (!modifiers.Any())
                return;

            var scriptContent = await _fileSystemAccess.ReadFileAsync(paths.DeployScriptPath);

            foreach (var m in modifiers.OrderBy(m => m.Key))
            {
                await Logger.LogAsync($"Modifying script: {m.Key}");

                scriptContent = await m.Value.ModifyAsync(scriptContent,
                                                          project,
                                                          configuration,
                                                          paths);
            }

            await _fileSystemAccess.WriteFileAsync(paths.DeployScriptPath, scriptContent);
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
            var stateModel = new ScriptCreationStateModel(project, configuration, previousVersion, latest, StateModelHandleWorkInProgressChanged);
            await DoWorkAsync(stateModel, cancellationToken);
            return stateModel.Result ?? false;
        }

        private async Task TryLoadSqlProjectProperties(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            var loaded = await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(stateModel.Project);
            if (!loaded)
                stateModel.Result = false;
        }

        private async Task FormatTargetVersion(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            if (stateModel.CreateLatest)
            {
                stateModel.FormattedTargetVersion = new Version(0, 0);
                return;
            }

            stateModel.FormattedTargetVersion = Version.Parse(_versionService.FormatVersion(stateModel.Project.ProjectProperties.DacVersion, stateModel.Configuration));

            // Check DacVersion against base version, if not running latest creation
            if (stateModel.FormattedTargetVersion > stateModel.PreviousVersion)
                return;

            stateModel.Result = false;
            await Logger.LogAsync($"ERROR: DacVersion of SQL project ({stateModel.FormattedTargetVersion}) is equal to or smaller than the previous version ({stateModel.PreviousVersion}).");
            _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
        }

        private async Task TryLoadPathsForScriptCreation(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            stateModel.Paths = await _sqlProjectService.TryLoadPathsForScriptCreationAsync(stateModel.Project, stateModel.Configuration, stateModel.PreviousVersion, stateModel.CreateLatest);
            if (stateModel.Paths == null)
                stateModel.Result = false;
        }

        private async Task VerifyPaths(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            await Logger.LogAsync("Verifying paths ...");

            stateModel.ArePathsVerified = true;
            if (_fileSystemAccess.CheckIfFileExists(stateModel.Paths.PublishProfilePath))
                return;

            stateModel.Result = false;
            await Logger.LogAsync("ERROR: Failed to find publish profile.");
        }

        private async Task TryBuildProject(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            if (stateModel.Configuration.BuildBeforeScriptCreation
                && !await _buildService.BuildProjectAsync(stateModel.Project))
                stateModel.Result = false;
            stateModel.HasTriedToBuildProject = true;
        }

        private async Task TryCopyBuildResult(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            if (!await _buildService.CopyBuildResultAsync(stateModel.Project, stateModel.Paths.NewDacpacDirectory))
                stateModel.Result = false;
            stateModel.HasTriedToCopyBuildResult = true;
        }

        private async Task TryToCreateDeploymentFiles(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            var success = await CreateScriptAsync(stateModel.Paths, stateModel.Configuration.CreateDocumentationWithScriptCreation);
            if (!success)
            {
                await Logger.LogAsync("ERROR: Script creation aborted.");
                stateModel.Result = false;
            }

            stateModel.HasTriedToCreateDeploymentFiles = true;
        }

        private async Task ModifyDeploymentScript(ScriptCreationStateModel stateModel, CancellationToken cancellationToken)
        {
            await ModifyCreatedScriptAsync(stateModel.Project, stateModel.Configuration, stateModel.Paths);
            stateModel.HasModifiedDeploymentScript = true;
        }

        protected override string GetOperationStartedMessage() => "Initializing script creation ...";

        protected override string GetOperationCompletedMessage(ScriptCreationStateModel stateModel, long elapsedMilliseconds)
        {
            return $"========== Script creation finished after {elapsedMilliseconds} milliseconds. ==========";
        }

        protected override string GetOperationFailedMessage(Exception exception) => $"ERROR: Script creation failed: {exception.Message}";

        protected override Func<ScriptCreationStateModel, CancellationToken, Task> GetNextWorkUnitForStateModel(ScriptCreationStateModel stateModel)
        {
            if (stateModel.Project.ProjectProperties.SqlTargetName == null)
                return TryLoadSqlProjectProperties;
            if (stateModel.FormattedTargetVersion == null)
                return FormatTargetVersion;
            if (stateModel.Paths == null)
                return TryLoadPathsForScriptCreation;
            if (!stateModel.ArePathsVerified)
                return VerifyPaths;
            if (!stateModel.HasTriedToBuildProject)
                return TryBuildProject;
            if (!stateModel.HasTriedToCopyBuildResult)
                return TryCopyBuildResult;
            if (!stateModel.HasTriedToCreateDeploymentFiles)
                return TryToCreateDeploymentFiles;
            if (!stateModel.HasModifiedDeploymentScript)
                return ModifyDeploymentScript;

            return null;
        }

        private async Task StateModelHandleWorkInProgressChanged(bool workInProgress)
        {
            IsCreating = workInProgress;
            if (IsCreating)
            {
                await _visualStudioAccess.StartLongRunningTaskIndicatorAsync();
                await _visualStudioAccess.ClearSSDTLifecycleOutputAsync();
            }
            else
            {
                try
                {
                    await _visualStudioAccess.StopLongRunningTaskIndicatorAsync();
                }
                catch
                {
                    // ignored
                }
            }
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