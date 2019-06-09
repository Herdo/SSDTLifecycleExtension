namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess
{
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface ILogger
    {
        Task LogAsync([NotNull] string message);
    }
}