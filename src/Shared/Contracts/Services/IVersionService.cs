namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IVersionService
{
    /// <summary>
    ///     Formats the <paramref name="version" /> given a specific <paramref name="configuration" />.
    /// </summary>
    /// <param name="version">The <see cref="Version" /> to format.</param>
    /// <param name="configuration">The <see cref="ConfigurationModel" /> used to format the <paramref name="version" />.</param>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="version" /> or p<paramref name="configuration" /> are
    ///     <b>null</b>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     The <paramref name="configuration" />.
    ///     <see cref="ConfigurationModel.VersionPattern" /> is not valid.
    /// </exception>
    /// <returns>The formatted version string.</returns>
    string FormatVersion([NotNull] Version version,
                         [NotNull] ConfigurationModel configuration);
}