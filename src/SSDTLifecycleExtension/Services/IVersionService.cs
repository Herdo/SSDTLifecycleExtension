namespace SSDTLifecycleExtension.Services
{
    using System;
    using Annotations;
    using Shared.Models;

    public interface IVersionService
    {
        string DetermineFinalVersion([NotNull] Version version,
                                     [NotNull] ConfigurationModel versionConfiguration);
    }
}