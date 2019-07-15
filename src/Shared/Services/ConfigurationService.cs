namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Services;
    using Events;
    using JetBrains.Annotations;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Task = System.Threading.Tasks.Task;

    [UsedImplicitly]
    public class ConfigurationService : DefaultContractResolver, IConfigurationService
    {
        private readonly IFileSystemAccess _fileSystemAccess;
        private readonly IVisualStudioAccess _visualStudioAccess;

        public ConfigurationService(IFileSystemAccess fileSystemAccess,
                                    IVisualStudioAccess visualStudioAccess)
        {
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
        }

        private static string GetConfigurationPath(SqlProject project)
        {
            var directory = Path.GetDirectoryName(project.FullName);
            return Path.Combine(directory ?? throw new InvalidOperationException("Cannot find configuration file. Directory is <null>."), "Properties", "ssdtlifecycle.json");
        }

        private async Task<ConfigurationModel> GetConfigurationOrDefaultInternalAsync(SqlProject project,
                                                                                      string path)
        {
            var sourcePath = path ?? GetConfigurationPath(project);
            var serialized = await _fileSystemAccess.ReadFileAsync(sourcePath);
            if (serialized == null)
            {
                var defaultInstance = ConfigurationModel.GetDefault();
                defaultInstance.ValidateAll();
                return defaultInstance;
            }

            var settings = new JsonSerializerSettings
            {
                ContractResolver = this
            };
            var deserialized = JsonConvert.DeserializeObject<ConfigurationModel>(serialized, settings);
            deserialized.ValidateAll();
            return deserialized;
        }

        private async Task SaveConfigurationInternalAsync(SqlProject project,
                                                          ConfigurationModel model)
        {
            var targetPath = GetConfigurationPath(project);
            var serialized = JsonConvert.SerializeObject(model, Formatting.Indented);

            // Save configuration physically.
            await _fileSystemAccess.WriteFileAsync(targetPath, serialized);

            // Notify about changes
            ConfigurationChanged?.Invoke(this, new ProjectConfigurationChangedEventArgs(project));

            // Add configuration to the project, if it hasn't been added before.
            _visualStudioAccess.AddItemToProjectProperties(project, targetPath);
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

        Task IConfigurationService.SaveConfigurationAsync(SqlProject project,
                                                          ConfigurationModel model)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return SaveConfigurationInternalAsync(project, model);
        }
    }
}