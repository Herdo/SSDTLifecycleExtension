namespace SSDTLifecycleExtension.Services
{
    using System;
    using System.Threading.Tasks;
    using Annotations;
    using Shared.Contracts;
    using Shared.Events;
    using Shared.Models;

    public interface IConfigurationService
    {
        event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

        Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] SqlProject project);

        Task SaveConfigurationAsync([NotNull] SqlProject project,
                                    [NotNull] ConfigurationModel model);
    }
}