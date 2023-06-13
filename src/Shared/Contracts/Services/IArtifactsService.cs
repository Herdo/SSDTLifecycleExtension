namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IArtifactsService
{
    VersionModel[] GetExistingArtifactVersions([NotNull] SqlProject project,
                                               [NotNull] ConfigurationModel configuration);
}