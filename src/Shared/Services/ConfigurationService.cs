namespace SSDTLifecycleExtension.Shared.Services;

[UsedImplicitly]
public class ConfigurationService : DefaultContractResolver, IConfigurationService
{
    private readonly IFileSystemAccess _fileSystemAccess;
    private readonly IVisualStudioAccess _visualStudioAccess;
    private readonly ILogger _logger;

    public ConfigurationService(IFileSystemAccess fileSystemAccess,
                                IVisualStudioAccess visualStudioAccess,
                                ILogger logger)
    {
        _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
        _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private static string GetConfigurationPath(SqlProject project)
    {
        var directory = Path.GetDirectoryName(project.FullName);
        return Path.Combine(directory ?? throw new InvalidOperationException("Cannot find configuration file. Directory is <null>."),
                            "Properties",
                            "ssdtlifecycle.json");
    }

    private static ConfigurationModel GetValidatedDefaultInstance()
    {
        var defaultInstance = ConfigurationModel.GetDefault();
        defaultInstance.ValidateAll();
        return defaultInstance;
    }

    private async Task<ConfigurationModel> GetConfigurationOrDefaultInternalAsync(SqlProject project,
                                                                                  string path)
    {
        var sourcePath = path ?? GetConfigurationPath(project);
        string serialized;
        try
        {
            if (!_fileSystemAccess.CheckIfFileExists(sourcePath))
                return GetValidatedDefaultInstance();

            serialized = await _fileSystemAccess.ReadFileAsync(sourcePath);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, $"Failed to read the configuration from file '{sourcePath}' - please ensure you have access to the file");
            _visualStudioAccess.ShowModalError("Accessing the configuration file failed. "
                                             + "Please check the SSDT Lifecycle output window for more details. "
                                             + "Falling back to default configuration.");
            return GetValidatedDefaultInstance();
        }

        var settings = new JsonSerializerSettings
        {
            ContractResolver = this
        };
        var deserialized = JsonConvert.DeserializeObject<ConfigurationModel>(serialized, settings);
        deserialized.ValidateAll();
        return deserialized;
    }

    private async Task<bool> SaveConfigurationInternalAsync(SqlProject project,
                                                            ConfigurationModel model)
    {
        var targetPath = GetConfigurationPath(project);
        var serialized = JsonConvert.SerializeObject(model, Newtonsoft.Json.Formatting.Indented);

        try
        {
            // Save configuration physically.
            await _fileSystemAccess.WriteFileAsync(targetPath, serialized);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to save the configuration");
            _visualStudioAccess.ShowModalError("Failed to save the configuration. Please check the SSDT Lifecycle output window for details.");
            return false;
        }

        // Notify about changes
        ConfigurationChanged?.Invoke(this, new ProjectConfigurationChangedEventArgs(project));

        // Add configuration to the project, if it hasn't been added before.
        _visualStudioAccess.AddItemToProjectProperties(project, targetPath);

        return true;
    }

    protected override JsonProperty CreateProperty(MemberInfo member,
                                                   MemberSerialization memberSerialization)
    {
        var defaultInstance = ConfigurationModel.GetDefault();
        var property = base.CreateProperty(member, memberSerialization);
        var defaultValue = typeof(ConfigurationModel).GetProperty(member.Name)?.GetValue(defaultInstance);
        if (defaultValue != null)
        {
            property.DefaultValueHandling = DefaultValueHandling.Populate;
            property.DefaultValue = defaultValue;
        }

        return property;
    }

    public event EventHandler<ProjectConfigurationChangedEventArgs> ConfigurationChanged;

    Task<ConfigurationModel> IConfigurationService.GetConfigurationOrDefaultAsync(SqlProject project)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));

        return GetConfigurationOrDefaultInternalAsync(project, null);
    }

    Task<ConfigurationModel> IConfigurationService.GetConfigurationOrDefaultAsync(string path)
    {
        if (path == null)
            throw new ArgumentNullException(nameof(path));

        return GetConfigurationOrDefaultInternalAsync(null, path);
    }

    Task<bool> IConfigurationService.SaveConfigurationAsync(SqlProject project,
                                                            ConfigurationModel model)
    {
        if (project == null)
            throw new ArgumentNullException(nameof(project));
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return SaveConfigurationInternalAsync(project, model);
    }
}