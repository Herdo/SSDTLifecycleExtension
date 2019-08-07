

namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Factories;
    using Contracts.Services;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ScriptCreationService : AsyncExecutorBase<ScriptCreationStateModel>, IScriptCreationService
    {
        [NotNull] private readonly IWorkUnitFactory _workUnitFactory;
        [NotNull] private readonly IVisualStudioAccess _visualStudioAccess;

        private bool _isCreating;

        public ScriptCreationService([NotNull] IWorkUnitFactory workUnitFactory,
                                     [NotNull] IVisualStudioAccess visualStudioAccess,
                                     [NotNull] ILogger logger)
            : base(logger)
        {
            _workUnitFactory = workUnitFactory ?? throw new ArgumentNullException(nameof(workUnitFactory));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
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

        protected override string GetOperationStartedMessage() => "Initializing script creation ...";

        protected override string GetOperationCompletedMessage(ScriptCreationStateModel stateModel, long elapsedMilliseconds)
        {
            return $"========== Script creation finished after {elapsedMilliseconds} milliseconds. ==========";
        }

        protected override string GetOperationFailedMessage() => "Script creation failed.";

        protected override IWorkUnit<ScriptCreationStateModel> GetNextWorkUnitForStateModel(ScriptCreationStateModel stateModel)
        {
            return _workUnitFactory.GetNextWorkUnit(stateModel);
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