namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using JetBrains.Annotations;
    using Models;

    public interface IArtifactsService
    {
        VersionModel[] GetExistingArtifactVersions([NotNull] SqlProject project,
                                                   [NotNull] ConfigurationModel configuration);
    }
}