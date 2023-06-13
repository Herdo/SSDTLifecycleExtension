namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IConfigurationService
{
    event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

    Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] SqlProject project);

    Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] string path);

    Task<bool> SaveConfigurationAsync([NotNull] SqlProject project,
                                      [NotNull] ConfigurationModel model);
}