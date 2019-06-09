namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using JetBrains.Annotations;
    using Models;

    public interface IVersionService
    {
        string DetermineFinalVersion([NotNull] Version version,
                                     [NotNull] ConfigurationModel versionConfiguration);
    }
}