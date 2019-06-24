namespace SSDTLifecycleExtension.Shared.WorkUnits
{
    using System;
    using Contracts;
    using Contracts.Enums;
    using Contracts.Factories;
    using Contracts.Models;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class WorkUnitFactory : IWorkUnitFactory
    {
        [NotNull] private readonly IDependencyResolver _dependencyResolver;

        public WorkUnitFactory([NotNull] IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        }

        IWorkUnit<ScaffoldingStateModel> IWorkUnitFactory.GetNextWorkUnit(ScaffoldingStateModel stateModel)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            switch (stateModel.CurrentState)
            {
                case StateModelState.Initialized:
                    return _dependencyResolver.Get<LoadSqlProjectPropertiesUnit>();
                case StateModelState.SqlProjectPropertiesLoaded:
                    return _dependencyResolver.Get<FormatTargetVersionUnit>();
                case StateModelState.FormattedTargetVersionLoaded:
                    return _dependencyResolver.Get<LoadPathsUnit>();
                case StateModelState.PathsLoaded:
                    return _dependencyResolver.Get<BuildProjectUnit>();
                case StateModelState.TriedToBuildProject:
                    return _dependencyResolver.Get<CopyBuildResultUnit>();
                case StateModelState.TriedToCopyBuildResult:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateModel) + '.' + nameof(IStateModel.CurrentState));
            }
        }

        IWorkUnit<ScriptCreationStateModel> IWorkUnitFactory.GetNextWorkUnit(ScriptCreationStateModel stateModel)
        {
            if (stateModel == null)
                throw new ArgumentNullException(nameof(stateModel));

            switch (stateModel.CurrentState)
            {
                case StateModelState.Initialized:
                    return _dependencyResolver.Get<LoadSqlProjectPropertiesUnit>();
                case StateModelState.SqlProjectPropertiesLoaded:
                    return _dependencyResolver.Get<FormatTargetVersionUnit>();
                case StateModelState.FormattedTargetVersionLoaded:
                    return _dependencyResolver.Get<ValidateTargetVersionUnit>();
                case StateModelState.FormattedTargetVersionValidated:
                    return _dependencyResolver.Get<LoadPathsUnit>();
                case StateModelState.PathsLoaded:
                    return _dependencyResolver.Get<VerifyPathsUnit>();
                case StateModelState.PathsVerified:
                    return _dependencyResolver.Get<BuildProjectUnit>();
                case StateModelState.TriedToBuildProject:
                    return _dependencyResolver.Get<CopyBuildResultUnit>();
                case StateModelState.TriedToCopyBuildResult:
                    return _dependencyResolver.Get<CreateDeploymentFilesUnit>();
                case StateModelState.TriedToCreateDeploymentFiles:
                    return _dependencyResolver.Get<ModifyDeploymentScriptUnit>();
                case StateModelState.ModifiedDeploymentScript:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateModel) + '.' + nameof(IStateModel.CurrentState));
            }
        }
    }
}