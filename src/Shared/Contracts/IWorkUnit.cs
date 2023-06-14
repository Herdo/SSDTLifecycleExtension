namespace SSDTLifecycleExtension.Shared.Contracts;

public interface IWorkUnit<in TStateModel>
    where TStateModel : IStateModel
{
    Task Work([NotNull] TStateModel stateModel,
              CancellationToken cancellationToken);
}