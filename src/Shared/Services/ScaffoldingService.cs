namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Services;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Factories;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ScaffoldingService : AsyncExecutorBase<ScaffoldingStateModel>, IScaffoldingService
    {
        [NotNull] private readonly IWorkUnitFactory _workUnitFactory;
        [NotNull] private readonly IVisualStudioAccess _visualStudioAccess;

        private bool _isScaffolding;

        public ScaffoldingService([NotNull] IWorkUnitFactory workUnitFactory,
                                  [NotNull] IVisualStudioAccess visualStudioAccess,
                                  [NotNull] ILogger logger)
            : base(logger)
        {
            _workUnitFactory = workUnitFactory ?? throw new ArgumentNullException(nameof(workUnitFactory));
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

        protected override string GetOperationStartedMessage() => "Initializing scaffolding ...";

        protected override string GetOperationCompletedMessage(ScaffoldingStateModel stateModel, long elapsedMilliseconds)
        {
            return $"========== Scaffolding version {stateModel.FormattedTargetVersion} finished after {elapsedMilliseconds} milliseconds. ==========";
        }

        protected override string GetOperationFailedMessage(Exception exception) => $"ERROR: DACPAC scaffolding failed: {exception.Message}";

        protected override IWorkUnit<ScaffoldingStateModel> GetNextWorkUnitForStateModel(ScaffoldingStateModel stateModel)
        {
            return _workUnitFactory.GetNextWorkUnit(stateModel);
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