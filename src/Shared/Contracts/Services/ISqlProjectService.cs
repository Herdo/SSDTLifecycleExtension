namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System.Threading.Tasks;

    public interface ISqlProjectService
    {
        Task<(string OutputPath, string SqlTargetName)> GetSqlProjectInformationAsync(string projectPath);
    }
}