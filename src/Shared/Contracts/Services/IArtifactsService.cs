namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IArtifactsService
{
    Task<VersionModel[]> GetExistingArtifactVersionsAsync(SqlProject project,
        ConfigurationModel configuration);
}