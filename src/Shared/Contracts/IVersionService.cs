namespace SSDTLifecycleExtension.Shared.Contracts
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