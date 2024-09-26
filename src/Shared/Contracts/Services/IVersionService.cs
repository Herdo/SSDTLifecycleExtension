namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IVersionService
{
    /// <summary>
    ///     Formats the <paramref name="version" /> given a specific <paramref name="configuration" />.
    /// </summary>
    /// <param name="version">The <see cref="Version" /> to format.</param>
    /// <param name="configuration">The <see cref="ConfigurationModel" /> used to format the <paramref name="version" />.</param>
    /// <exception cref="InvalidOperationException">
    ///     The <paramref name="configuration" />.
    ///     <see cref="ConfigurationModel.VersionPattern" /> is not valid.
    /// </exception>
    /// <returns>The formatted version string.</returns>
    string FormatVersion(Version version,
        ConfigurationModel configuration);
}