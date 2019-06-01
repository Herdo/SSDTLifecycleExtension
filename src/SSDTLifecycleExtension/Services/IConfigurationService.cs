namespace SSDTLifecycleExtension.Services
{
    using System;
    using System.Threading.Tasks;
    using Annotations;
    using EnvDTE;
    using Events;
    using Shared.Models;

    public interface IConfigurationService
    {
        event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

        Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] Project project);

        Task SaveConfigurationAsync([NotNull] Project project,
                                    [NotNull] ConfigurationModel model);
    }
}