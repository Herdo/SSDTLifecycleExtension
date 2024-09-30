namespace SSDTLifecycleExtension.Shared.Services;

public class ArtifactsService(IVisualStudioAccess _visualStudioAccess,
                              IFileSystemAccess _fileSystemAccess)
    : IArtifactsService
{
    VersionModel[] IArtifactsService.GetExistingArtifactVersions(SqlProject project,
        ConfigurationModel configuration)
    {
        return GetExistingArtifactVersionsInternal(project, configuration);
    }

    private VersionModel[] GetExistingArtifactVersionsInternal(SqlProject project,
        ConfigurationModel configuration)
    {
        if (!TryGetArtifactsBaseDirectory(project, configuration, out var artifactsBaseDirectory))
            return [];

        if (!TryGetArtifactDirectories(artifactsBaseDirectory!, out var artifactDirectories))
            return [];

        var existingVersions = ParseExistingDirectories(artifactDirectories!);
        var versionModels = CreateModels(existingVersions);
        DetermineHighestExistingVersion(versionModels);
        return versionModels;
    }

    private bool TryGetArtifactsBaseDirectory(SqlProject project,
        ConfigurationModel configuration,
        out string? artifactsBaseDirectory)
    {
        var projectPath = project.FullName;
        var projectDirectory = Path.GetDirectoryName(projectPath);
        if (projectDirectory == null)
        {
            _visualStudioAccess.ShowModalError("ERROR: Cannot determine project directory.");
            artifactsBaseDirectory = null;
            return false;
        }

        var artifactPathErrors = ConfigurationModelValidations.ValidateArtifactsPath(configuration);
        if (artifactPathErrors.Any())
        {
            _visualStudioAccess.ShowModalError("ERROR: The configured artifacts path is not valid. Please ensure that the configuration is correct.");
            artifactsBaseDirectory = null;
            return false;
        }

        artifactsBaseDirectory = Path.Combine(projectDirectory, configuration.ArtifactsPath);
        return true;
    }

    private bool TryGetArtifactDirectories(string artifactsBaseDirectory,
        out string[]? artifactDirectories)
    {
        try
        {
            artifactDirectories = _fileSystemAccess.GetDirectoriesIn(artifactsBaseDirectory);
        }
        catch (Exception e)
        {
            _visualStudioAccess.ShowModalError($"ERROR: Failed to open script creation window: {e.Message}");
            artifactDirectories = null;
            return false;
        }

        return true;
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