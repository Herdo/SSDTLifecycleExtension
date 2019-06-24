namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Services;
    using Contracts;
    using Contracts.DataAccess;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ScaffoldingService : AsyncExecutorBase<ScaffoldingStateModel>, IScaffoldingService
    {
        private readonly ISqlProjectService _sqlProjectService;
        private readonly IBuildService _buildService;
        private readonly IVersionService _versionService;
        private readonly IVisualStudioAccess _visualStudioAccess;

        private bool _isScaffolding;

        public ScaffoldingService(ISqlProjectService sqlProjectService,
                                  IBuildService buildService,
                                  IVersionService versionService,
                                  IVisualStudioAccess visualStudioAccess,
                                  ILogger logger)
            :base(logger)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        }

        private async Task<bool> ScaffoldInternalAsync(SqlProject project,
                                                       ConfigurationModel configuration,
                                                       Version targetVersion,
                                                       CancellationToken cancellationToken)
        {
            var stateModel = new ScaffoldingStateModel(project, configuration, targetVersion, StateModelHandleWorkInProgressChanged);
            await DoWorkAsync(stateModel, cancellationToken);
            return stateModel.Result ?? false;
        }

        private async Task TryLoadSqlProjectProperties(ScaffoldingStateModel stateModel, CancellationToken cancellationToken)
        {
            var loaded = await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(stateModel.Project);
            if (!loaded)
                stateModel.Result = false;
        }

        private async Task FormatTargetVersion(ScaffoldingStateModel stateModel, CancellationToken cancellationToken)
        {
            stateModel.FormattedTargetVersion = Version.Parse(_versionService.FormatVersion(stateModel.TargetVersion, stateModel.Configuration));

            // Check DacVersion against planned target version
            if (stateModel.Project.ProjectProperties.DacVersion == stateModel.FormattedTargetVersion)
                return;

            stateModel.Result = false;
            await Logger.LogAsync($"ERROR: DacVersion of SQL project ({stateModel.Project.ProjectProperties.DacVersion}) doesn't match target version ({stateModel.FormattedTargetVersion}).");
            _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
        }

        private async Task TryLoadPathsForScaffolding(ScaffoldingStateModel stateModel, CancellationToken cancellationToken)
        {
            stateModel.Paths = await _sqlProjectService.TryLoadPathsForScaffoldingAsync(stateModel.Project, stateModel.Configuration);
            if (stateModel.Paths == null)
                stateModel.Result = false;
        }

        private async Task TryBuildProject(ScaffoldingStateModel stateModel, CancellationToken cancellationToken)
        {
            if (!await _buildService.BuildProjectAsync(stateModel.Project))
                stateModel.Result = false;
            stateModel.HasTriedToBuildProject = true;
        }

        private async Task TryCopyBuildResult(ScaffoldingStateModel stateModel, CancellationToken cancellationToken)
        {
            if (!await _buildService.CopyBuildResultAsync(stateModel.Project, stateModel.Paths.NewDacpacDirectory))
                stateModel.Result = false;
            stateModel.HasTriedToCopyBuildResult = true;
        }

        protected override string GetOperationStartedMessage() => "Initializing scaffolding ...";

        protected override string GetOperationCompletedMessage(ScaffoldingStateModel stateModel, long elapsedMilliseconds)
        {
            return $"========== Scaffolding version {stateModel.FormattedTargetVersion} finished after {elapsedMilliseconds} milliseconds. ==========";
        }

        protected override string GetOperationFailedMessage(Exception exception) => $"ERROR: DACPAC scaffolding failed: {exception.Message}";

        protected override Func<ScaffoldingStateModel, CancellationToken, Task> GetNextWorkUnitForStateModel(ScaffoldingStateModel stateModel)
        {
            if (stateModel.Project.ProjectProperties.SqlTargetName == null)
                return TryLoadSqlProjectProperties;
            if (stateModel.FormattedTargetVersion == null)
                return FormatTargetVersion;
            if (stateModel.Paths == null)
                return TryLoadPathsForScaffolding;
            if (!stateModel.HasTriedToBuildProject)
                return TryBuildProject;
            if (!stateModel.HasTriedToCopyBuildResult)
                return TryCopyBuildResult;

            return null;
        }

        private async Task StateModelHandleWorkInProgressChanged(bool workInProgress)
        {
            IsScaffolding = workInProgress;
            if (IsScaffolding)
            {
                _visualStudioAccess.StartLongRunningTaskIndicatorAsync().Wait();
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

        public event EventHandler IsScaffoldingChanged;

        private bool IsScaffolding
        {
            get => _isScaffolding;
            set
            {
                if (value == _isScaffolding) return;
                _isScaffolding = value;
                IsScaffoldingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        bool IScaffoldingService.IsScaffolding => IsScaffolding;

        Task<bool> IScaffoldingService.ScaffoldAsync(SqlProject project,
                                                     ConfigurationModel configuration,
                                                     Version targetVersion,
                                                     CancellationToken cancellationToken)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (targetVersion == null)
                throw new ArgumentNullException(nameof(targetVersion));
            if (IsScaffolding)
                throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

            return ScaffoldInternalAsync(project, configuration, targetVersion, cancellationToken);
        }
    }
}