using System.Text.Json;

namespace SSDTLifecycleExtension.Shared.Services;

public class ConfigurationService(IFileSystemAccess _fileSystemAccess,
                                  IVisualStudioAccess _visualStudioAccess,
                                  ILogger _logger)
    : IConfigurationService
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

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

    private async Task<ConfigurationModel> GetConfigurationOrDefaultInternalAsync(SqlProject? project,
                                                                                  string? path)
    {
        var sourcePath = path ?? GetConfigurationPath(project!);
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
            await _visualStudioAccess.ShowModalErrorAsync("Accessing the configuration file failed. "
                + "Please check the SSDT Lifecycle output window for more details. "
                + "Falling back to default configuration.");
            return GetValidatedDefaultInstance();
        }

        var deserialized = JsonSerializer.Deserialize<ConfigurationModel>(serialized, _jsonSerializerOptions);
        if (deserialized is null)
        {
            await _logger.LogErrorAsync($"Failed to deserialize the configuration from file '{sourcePath}' - falling back to default configuration");
            await _visualStudioAccess.ShowModalErrorAsync("Deserializing the configuration file failed. "
                + "Please check the SSDT Lifecycle output window for more details. "
                + "Falling back to default configuration.");
            return GetValidatedDefaultInstance();
        }
        deserialized.ValidateAll();

        if (!deserialized.HasErrors && deserialized.WasUpgraded)
        {
            await SaveConfigurationInternalAsync(project!, deserialized, false);
        }

        return deserialized;
    }

    private async Task<bool> SaveConfigurationInternalAsync(SqlProject project,
        ConfigurationModel model,
        bool notifyChanged)
    {
        var targetPath = GetConfigurationPath(project);
        var serialized = JsonSerializer.Serialize(model, _jsonSerializerOptions);

        try
        {
            // Save configuration physically.
            await _fileSystemAccess.WriteFileAsync(targetPath, serialized);
        }
        catch (Exception e)
        {
            await _logger.LogErrorAsync(e, "Failed to save the configuration");
            await _visualStudioAccess.ShowModalErrorAsync("Failed to save the configuration. Please check the SSDT Lifecycle output window for details.");
            return false;
        }

        if (notifyChanged)
            ConfigurationChanged?.Invoke(this, new ProjectConfigurationChangedEventArgs(project));

        // Add configuration to the project, if it hasn't been added before.
        await _visualStudioAccess.AddConfigFileToProjectPropertiesAsync(project, targetPath);

        return true;
    }

    public event EventHandler<ProjectConfigurationChangedEventArgs>? ConfigurationChanged;

    Task<ConfigurationModel> IConfigurationService.GetConfigurationOrDefaultAsync(SqlProject project)
    {
        return GetConfigurationOrDefaultInternalAsync(project, null);
    }

    Task<ConfigurationModel> IConfigurationService.GetConfigurationOrDefaultAsync(string path)
    {
        return GetConfigurationOrDefaultInternalAsync(null, path);
    }

    Task<bool> IConfigurationService.SaveConfigurationAsync(SqlProject project,
                                                            ConfigurationModel model)
    {
        return SaveConfigurationInternalAsync(project, model, true);
    }
}