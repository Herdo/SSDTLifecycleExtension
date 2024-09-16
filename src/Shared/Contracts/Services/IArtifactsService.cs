namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IArtifactsService
{
    VersionModel[] GetExistingArtifactVersions(SqlProject project,
        ConfigurationModel configuration);
}