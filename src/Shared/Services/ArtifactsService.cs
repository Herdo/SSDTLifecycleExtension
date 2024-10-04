namespace SSDTLifecycleExtension.Shared.Services;

public class ArtifactsService(IVisualStudioAccess _visualStudioAccess,
                              IFileSystemAccess _fileSystemAccess)
    : IArtifactsService
{
    async Task<VersionModel[]> IArtifactsService.GetExistingArtifactVersionsAsync(SqlProject project,
        ConfigurationModel configuration)
    {
        return await GetExistingArtifactVersionsInternalAsync(project, configuration);
    }

    private async Task<VersionModel[]> GetExistingArtifactVersionsInternalAsync(SqlProject project,
        ConfigurationModel configuration)
    {
        var artifactsBaseDirectory = await TryGetArtifactsBaseDirectoryAsync(project, configuration);
        if (artifactsBaseDirectory is null)
            return [];

        var artifactDirectories = await TryGetArtifactDirectoriesAsync(artifactsBaseDirectory);
        if (artifactDirectories is null)
            return [];

        var existingVersions = ParseExistingDirectories(artifactDirectories!);
        var versionModels = CreateModels(existingVersions);
        DetermineHighestExistingVersion(versionModels);
        return versionModels;
    }

    private async Task<string?> TryGetArtifactsBaseDirectoryAsync(SqlProject project,
        ConfigurationModel configuration)
    {
        var projectPath = project.FullName;
        var projectDirectory = Path.GetDirectoryName(projectPath);
        if (projectDirectory == null)
        {
            await _visualStudioAccess.ShowModalErrorAsync("ERROR: Cannot determine project directory.");
            return null;
        }

        var artifactPathErrors = ConfigurationModelValidations.ValidateArtifactsPath(configuration);
        if (artifactPathErrors.Any())
        {
            await _visualStudioAccess.ShowModalErrorAsync("ERROR: The configured artifacts path is not valid. Please ensure that the configuration is correct.");
            return null;
        }

        return Path.Combine(projectDirectory, configuration.ArtifactsPath);
    }

    private async Task<string[]?> TryGetArtifactDirectoriesAsync(string artifactsBaseDirectory)
    {
        try
        {
            return _fileSystemAccess.GetDirectoriesIn(artifactsBaseDirectory);
        }
        catch (Exception e)
        {
            await _visualStudioAccess.ShowModalErrorAsync($"ERROR: Failed to open script creation window: {e.Message}");
            return null;
        }
    }

    private static IEnumerable<Version> ParseExistingDirectories(IEnumerable<string> artifactDirectories)
    {
        var existingVersions = new List<Version>();
        foreach (var artifactDirectory in artifactDirectories)
        {
            var di = new DirectoryInfo(artifactDirectory);
            if (Version.TryParse(di.Name, out var existingVersion))
                existingVersions.Add(existingVersion);
        }

        return existingVersions;
    }

    private static VersionModel[] CreateModels(IEnumerable<Version> existingVersions)
    {
        return existingVersions.OrderByDescending(m => m)
                               .Select(version => new VersionModel
                               {
                                   UnderlyingVersion = version
                               })
                               .ToArray();
    }

    private static void DetermineHighestExistingVersion(VersionModel[] existingVersions)
    {
        if (existingVersions.Length == 0)
            return;
        var highestVersion = existingVersions.Select(m => m.UnderlyingVersion).Max();
        var highestModel = existingVersions.Single(m => m.UnderlyingVersion == highestVersion);
        highestModel.IsNewestVersion = true;
    }
}