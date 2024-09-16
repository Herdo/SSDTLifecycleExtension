namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface ISqlProjectService
{
    /// <summary>
    ///     Tries to load the <see cref="SqlProject.ProjectProperties" /> of the <paramref name="project" />.
    /// </summary>
    /// <param name="project">The <see cref="SqlProject" /> to load the <see cref="SqlProjectProperties" /> for.</param>
    /// <returns><b>True</b>, if the properties were loaded successfully, otherwise <b>false</b>.</returns>
    Task<bool> TryLoadSqlProjectPropertiesAsync(SqlProject project);

    Task<PathCollection?> TryLoadPathsForScaffoldingAsync(SqlProject project,
        ConfigurationModel configuration);

    Task<PathCollection?> TryLoadPathsForScriptCreationAsync(SqlProject project,
        ConfigurationModel configuration,
        Version previousVersion,
        bool createLatest);
}