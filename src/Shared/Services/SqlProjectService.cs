namespace SSDTLifecycleExtension.Shared.Services;

[UsedImplicitly]
public class SqlProjectService : ISqlProjectService
{
    private readonly IVersionService _versionService;
    private readonly IFileSystemAccess _fileSystemAccess;
    private readonly ILogger _logger;

    public SqlProjectService(IVersionService versionService,
                             IFileSystemAccess fileSystemAccess,
                             ILogger logger)
    {
        _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
        _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private static void ReadProperties(XContainer root,
                                       out string name,
                                       out string outputPath,
                                       out string sqlTargetName,
                                       out string dacVersion)
    {
        name = null;
        outputPath = null;
        sqlTargetName = null;
        dacVersion = null;

        var propertyGroups = root.Elements().Where(m => m.Name.LocalName == "PropertyGroup").ToArray();
        foreach (var propertyGroup in propertyGroups)
        {
            // If the property group has a condition, check if that condition contains "Release", otherwise skip this group
            var conditionAttribute = propertyGroup.Attribute("Condition");
            if (conditionAttribute != null && !conditionAttribute.Value.Contains("Release"))
                continue;

            var nameElement = propertyGroup.Elements().SingleOrDefault(m => m.Name.LocalName == "Name");
            if (nameElement != null)
                name = nameElement.Value;

            var outputPathElement = propertyGroup.Elements().SingleOrDefault(m => m.Name.LocalName == "OutputPath");
            if (outputPathElement != null)
                outputPath = outputPathElement.Value;

            var sqlTargetNameElement = propertyGroup.Elements().SingleOrDefault(m => m.Name.LocalName == "SqlTargetName");
            if (sqlTargetNameElement != null)
                sqlTargetName = sqlTargetNameElement.Value;

            var dacVersionAttribute = propertyGroup.Elements().SingleOrDefault(m => m.Name.LocalName == "DacVersion");
            if (dacVersionAttribute != null)
                dacVersion = dacVersionAttribute.Value;
        }
    }


    private async Task<PathCollection> DeterminePathsAsync([NotNull] SqlProject project,
                                                           [NotNull] ConfigurationModel configuration,
                                                           [CanBeNull] Version previousVersion,
                                                           bool createLatest)
    {
        var projectPath = project.FullName;
        var projectDirectory = Path.GetDirectoryName(projectPath);
        if (projectDirectory == null)
        {
            await _logger.LogErrorAsync($"Cannot get project directory for {project.FullName}");
            return null;
        }

        // Versions
        const string latestKeyword = "latest";
        var previousVersionString = previousVersion == null ? null : _versionService.FormatVersion(previousVersion, configuration);
        var newVersionString = createLatest ? latestKeyword : _versionService.FormatVersion(project.ProjectProperties.DacVersion, configuration);

        // Directories
        var artifactsPath = Path.Combine(projectDirectory, configuration.ArtifactsPath);
        DirectoryPaths directories;
        {
            var latestDirectory = Path.Combine(artifactsPath, latestKeyword);
            var newVersionDirectory = Path.Combine(artifactsPath, newVersionString);
            directories = new DirectoryPaths(projectDirectory,
                                             latestDirectory,
                                             newVersionDirectory);
        }

        // Source paths
        var newVersionPath = Path.Combine(directories.NewArtifactsDirectory, $"{project.ProjectProperties.SqlTargetName}.dacpac");
        var profilePath = DeterminePublishProfilePath(configuration, projectDirectory);
        var previousVersionDirectory = previousVersion == null ? null : Path.Combine(artifactsPath, previousVersionString);
        var previousVersionPath = previousVersion == null ? null : Path.Combine(previousVersionDirectory, $"{project.ProjectProperties.SqlTargetName}.dacpac");
        var sources = new DeploySourcePaths(newVersionPath,
                                            profilePath,
                                            previousVersionPath);

        // Target paths
        var deployScriptPath = previousVersion == null
            ? null
            : Path.Combine(directories.NewArtifactsDirectory, $"{project.ProjectProperties.SqlTargetName}_{previousVersionString}_{newVersionString}.sql");
        var deployReportPath = previousVersion != null // Can only create report when comparing against a previous version
                            && configuration.CreateDocumentationWithScriptCreation
            ? Path.Combine(directories.NewArtifactsDirectory, $"{project.ProjectProperties.SqlTargetName}_{previousVersionString}_{newVersionString}.xml")
            : null;
        var targets = new DeployTargetPaths(deployScriptPath, deployReportPath);

        return new PathCollection(directories,
                                  sources,
                                  targets);
    }

    private string DeterminePublishProfilePath(ConfigurationModel configuration,
                                               string projectDirectory)
    {
        if (configuration.PublishProfilePath != ConfigurationModel.UseSinglePublishProfileSpecialKeyword)
            return Path.Combine(projectDirectory, configuration.PublishProfilePath);

        var publishProfiles = _fileSystemAccess.GetFilesIn(projectDirectory, "*.publish.xml");
        return publishProfiles.Length == 1
            ? Path.Combine(projectDirectory, publishProfiles[0])
            : string.Empty;
    }

    private async Task<bool> TryLoadSqlProjectPropertiesInternalAsync(SqlProject project)
    {
        var projectDirectory = Path.GetDirectoryName(project.FullName);
        if (projectDirectory == null)
        {
            await _logger.LogErrorAsync($"Cannot get project directory for {project.FullName}");
            return false;
        }

        string content;
        try
        {
            content = await _fileSystemAccess.ReadFileAsync(project.FullName);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to read the project file");
            return false;
        }

        XElement root;
        try
        {
            var doc = XDocument.Parse(content);
            // When using XDocument.Parse, the Root element must exist, otherwise an XmlException would be thrown.
            // Therefore there's no need to check if doc.Root is null.
            root = doc.Root;
        }
        catch (XmlException e)
        {
            await _logger.LogErrorAsync(e, $"Cannot read contents of \"{project.FullName}\"");
            return false;
        }

        ReadProperties(root,
                       out var name,
                       out var outputPath,
                       out var sqlTargetName,
                       out var dacVersion);

        if (name == null)
        {
            await _logger.LogErrorAsync($"Cannot read name of \"{project.FullName}\". "
                                      + "Please make sure that the \"Name\" is set correctly, e.g. \"MyDatabaseProject\". "
                                      + "This value has to be set manually in XML.");
            return false;
        }

        if (outputPath == null)
        {
            await _logger.LogErrorAsync($"Cannot read output path of \"{project.FullName}\". "
                                      + "Please make sure that the \"OutputPath\" for the current configuration is set correctly, e.g. \"bin\\Output\\\". "
                                      + "This value can be set from your database project => \"Properties\" => \"Build\" => \"Output path\".");
            return false;
        }

        if (dacVersion == null)
        {
            await _logger.LogErrorAsync($"Cannot read DacVersion of \"{project.FullName}\". "
                                      + "Please make sure that the \"DacVersion\" is set correctly, e.g. \"1.0.0\". "
                                      + "This value can bet set from your database project => \"Properties\" => \"Project Settings\" => \"Output types\" => \"Data-tier Application\" => \"Properties...\" => \"Version\".");
            return false;
        }

        // Verify XML 'Name' property
        if (name != project.Name)
        {
            await _logger.LogWarningAsync("XML node 'Name' doesn't match the actual project name. This could cause an unexpected behavior.");
            await _logger.LogDebugAsync($"Value of 'Name' node: {name}");
            await _logger.LogDebugAsync($"Actual project name: {project.Name}");
        }

        // Set properties on the project object
        project.ProjectProperties.SqlTargetName = string.IsNullOrWhiteSpace(sqlTargetName) ? name : sqlTargetName;
        project.ProjectProperties.BinaryDirectory = Path.Combine(projectDirectory, outputPath);
        project.ProjectProperties.DacVersion = Version.Parse(dacVersion);

        return true;
    }

    Task<bool> ISqlProjectService.TryLoadSqlProjectPropertiesAsync(SqlProject project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        return TryLoadSqlProjectPropertiesInternalAsync(project);
    }

    Task<PathCollection> ISqlProjectService.TryLoadPathsForScaffoldingAsync(SqlProject project,
                                                                            ConfigurationModel configuration)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return DeterminePathsAsync(project, configuration, null, false);
    }

    Task<PathCollection> ISqlProjectService.TryLoadPathsForScriptCreationAsync(SqlProject project,
                                                                               ConfigurationModel configuration,
                                                                               Version previousVersion,
                                                                               bool createLatest)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        if (previousVersion == null)
            throw new ArgumentNullException(nameof(previousVersion));

        return DeterminePathsAsync(project, configuration, previousVersion, createLatest);
    }
}