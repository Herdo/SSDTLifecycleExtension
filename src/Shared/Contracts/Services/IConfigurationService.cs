namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Events;
    using Models;
    using Shared;

    public interface IConfigurationService
    {
        event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

        Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] SqlProject project);

        Task SaveConfigurationAsync([NotNull] SqlProject project,
                                    [NotNull] ConfigurationModel model);
    }
}