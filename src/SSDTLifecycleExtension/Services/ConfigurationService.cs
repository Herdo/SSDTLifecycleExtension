namespace SSDTLifecycleExtension.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Annotations;
    using DataAccess;
    using EnvDTE;
    using Microsoft.VisualStudio.Shell;
    using Newtonsoft.Json;
    using Shared.Models;
    using Task = System.Threading.Tasks.Task;

    [UsedImplicitly]
    public class ConfigurationService : IConfigurationService
    {
        private const string _PROPERTIES_DIRECTORY = "Properties";
        private const string _CONFIGURATION_FILE_NAME = "ssdtlifecycle.json";

        private readonly IFileSystemAccess _fileSystemAccess;

        public ConfigurationService(IFileSystemAccess fileSystemAccess)
        {
            _fileSystemAccess = fileSystemAccess;
        }

        private static string GetConfigurationPath(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var directory = Path.GetDirectoryName(project.FullName);
            return Path.Combine(directory ?? throw new InvalidOperationException("Cannot find configuration file. Directory is <null>."), _PROPERTIES_DIRECTORY, _CONFIGURATION_FILE_NAME);
        }

        async Task<ConfigurationModel> IConfigurationService.GetConfigurationOrDefaultAsync(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var sourcePath = GetConfigurationPath(project);
            var serialized = await _fileSystemAccess.ReadFileAsync(sourcePath);
            return serialized == null
                       ? ConfigurationModel.GetDefault()
                       : JsonConvert.DeserializeObject<ConfigurationModel>(serialized);
        }

        async Task IConfigurationService.SaveConfigurationAsync(Project project,
                                                                ConfigurationModel model)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var targetPath = GetConfigurationPath(project);
            var serialized = JsonConvert.SerializeObject(model, Formatting.Indented);

            // Save configuration physically.
            await _fileSystemAccess.WriteFileAsync(targetPath, serialized);

            // Add configuration to the project, if it hasn't been added before.
            var properties = project.ProjectItems.OfType<ProjectItem>().SingleOrDefault(m =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return m.Name == _PROPERTIES_DIRECTORY;
            });
            if (properties == null)
                return;

            if (properties.ProjectItems.OfType<ProjectItem>().All(m =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return m.Name != _CONFIGURATION_FILE_NAME;
            })) properties.ProjectItems.AddFromFile(targetPath);
        }
    }
}