namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.Models;

    public interface IScaffoldingService
    {
        event EventHandler IsScaffoldingChanged;

        /// <summary>
        /// Gets whether a <see cref="ScaffoldAsync"/>-<see cref="Task"/> is currently running.
        /// </summary>
        bool IsScaffolding { get; }

        /// <summary>
        /// Scaffolds the <paramref name="project"/> using the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="project">The project so scaffold.</param>
        /// <param name="configuration">The configuration to use.</param>
        /// <param name="targetVersion">The version that will be created.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel the script creation.</param>
        /// <exception cref="ArgumentNullException"><paramref name="project"/>, <paramref name="configuration"/>, or <paramref name="targetVersion"/> are <b>null</b>.</exception>
        /// <exception cref="InvalidOperationException">A scaffolding is currently running. Check <see cref="IsScaffolding"/> before calling.</exception>
        /// <returns><b>True</b>, if scaffolding was successful, otherwise <b>false</b>.</returns>
        Task<bool> ScaffoldAsync([NotNull] SqlProject project,
                                 [NotNull] ConfigurationModel configuration,
                                 [NotNull] Version targetVersion,
                                 CancellationToken cancellationToken);
    }
}