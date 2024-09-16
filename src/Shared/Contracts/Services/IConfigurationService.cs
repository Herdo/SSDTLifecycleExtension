namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IConfigurationService
{
    event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

    Task<ConfigurationModel> GetConfigurationOrDefaultAsync(SqlProject project);

    Task<ConfigurationModel> GetConfigurationOrDefaultAsync(string path);

    Task<bool> SaveConfigurationAsync(SqlProject project,
        ConfigurationModel model);
}