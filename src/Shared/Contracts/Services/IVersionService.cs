namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using Models;
    using Shared;

    public interface IVersionService
    {
        string DetermineFinalVersion([NotNull] Version version,
                                     [NotNull] ConfigurationModel versionConfiguration);
    }
}