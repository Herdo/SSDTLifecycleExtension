namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IScriptCreationService
{
    event EventHandler IsCreatingChanged;

    /// <summary>
    ///     Gets whether a <see cref="CreateAsync" />-<see cref="Task" /> is currently running.
    /// </summary>
    bool IsCreating { get; }

    /// <summary>
    ///     Creates a new script for the given <paramref name="project" /> using the given <paramref name="configuration" />.
    /// </summary>
    /// <param name="project">The project to generate the script for.</param>
    /// <param name="configuration">The configuration to use.</param>
    /// <param name="previousVersion">The previous version used as base version.</param>
    /// <param name="latest">Whether to generate "latest" or the configured DacVersion.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> used to cancel the script creation.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="project" />, <paramref name="configuration" />, or
    ///     <paramref name="previousVersion" /> are <b>null</b>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     A creation is currently running. Check <see cref="IsCreating" /> before
    ///     calling.
    /// </exception>
    /// <returns><b>True</b>, if the creation was successful, otherwise <b>false</b>.</returns>
    Task<bool> CreateAsync([NotNull] SqlProject project,
                           [NotNull] ConfigurationModel configuration,
                           [NotNull] Version previousVersion,
                           bool latest,
                           CancellationToken cancellationToken);
}