namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess
{
    using System.Threading.Tasks;

    public interface ILogger
    {
        Task LogAsync([NotNull] string message);
    }
}