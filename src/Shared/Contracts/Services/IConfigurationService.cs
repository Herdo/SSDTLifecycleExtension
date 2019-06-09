﻿namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Events;
    using JetBrains.Annotations;
    using Models;

    public interface IConfigurationService
    {
        event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

        Task<ConfigurationModel> GetConfigurationOrDefaultAsync([NotNull] SqlProject project);

        Task SaveConfigurationAsync([NotNull] SqlProject project,
                                    [NotNull] ConfigurationModel model);
    }
}