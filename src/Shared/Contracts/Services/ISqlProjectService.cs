namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System.Threading.Tasks;

    public interface ISqlProjectService
    {
        Task LoadSqlProjectPropertiesAsync(SqlProject project);
    }
}