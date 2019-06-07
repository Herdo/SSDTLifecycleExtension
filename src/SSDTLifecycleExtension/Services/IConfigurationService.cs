namespace SSDTLifecycleExtension.Services
{
    using System;
    using System.Threading.Tasks;
    using Annotations;
    using Events;
    using Shared.Contracts;
    using Shared.Models;

    public interface IConfigurationService
    {
        event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

        Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] SqlProject project);

        Task SaveConfigurationAsync([NotNull] SqlProject project,
                                    [NotNull] ConfigurationModel model);
    }
}