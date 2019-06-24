namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ValidateTargetVersionUnit : IWorkUnit<ScaffoldingStateModel>,
                                             IWorkUnit<ScriptCreationStateModel>
    {
        [NotNull] private readonly IVisualStudioAccess _visualStudioAccess;
        [NotNull] private readonly ILogger _logger;

        public ValidateTargetVersionUnit([NotNull] IVisualStudioAccess visualStudioAccess,
                                         [NotNull] ILogger logger)
        {
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        async Task IWorkUnit<ScaffoldingStateModel>.Work(ScaffoldingStateModel stateModel,
                                                         CancellationToken cancellationToken)
        {
            // Check DacVersion against planned target version
            if (stateModel.Project.ProjectProperties.DacVersion == stateModel.FormattedTargetVersion)
            {
                stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
                return;
            }

            stateModel.Result = false;
            stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
            await _logger.LogAsync($"ERROR: DacVersion of SQL project ({stateModel.Project.ProjectProperties.DacVersion}) doesn't match target version ({stateModel.FormattedTargetVersion}).");
            _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
        }

        async Task IWorkUnit<ScriptCreationStateModel>.Work(ScriptCreationStateModel stateModel,
                                                            CancellationToken cancellationToken)
        {

            // Check DacVersion against base version, if not running latest creation
            if (stateModel.FormattedTargetVersion > stateModel.PreviousVersion)
            {
                stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
                return;
            }

            stateModel.Result = false;
            stateModel.CurrentState = StateModelState.FormattedTargetVersionValidated;
            await _logger.LogAsync($"ERROR: DacVersion of SQL project ({stateModel.FormattedTargetVersion}) is equal to or smaller than the previous version ({stateModel.PreviousVersion}).");
            _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
        }
    }
}