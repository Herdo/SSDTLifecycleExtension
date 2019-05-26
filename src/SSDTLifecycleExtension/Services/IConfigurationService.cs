namespace SSDTLifecycleExtension.Services
{
    using System.Threading.Tasks;
    using Annotations;
    using EnvDTE;
    using Models;

    public interface IConfigurationService
    {
        Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] Project project);

        Task SaveConfigurationAsync([NotNull] Project project,
                                    [NotNull] ConfigurationModel model);
    }
}