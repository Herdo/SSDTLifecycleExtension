namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using JetBrains.Annotations;
    using Shared.Models;

    public interface IArtifactsService
    {
        VersionModel[] GetExistingArtifactVersions([NotNull] SqlProject project,
                                                   [NotNull] ConfigurationModel configuration);
    }
}