namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Models;

    public interface IWorkUnit<in TStateModel>
        where TStateModel : IStateModel
    {
        Task Work([NotNull] TStateModel stateModel,
                  CancellationToken cancellationToken);
    }
}