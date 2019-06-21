﻿namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Linq;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Services;
    using JetBrains.Annotations;
    using Models;

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
                await _logger.LogAsync($"ERROR: Cannot get project directory for {project.FullName}");
                return null;
            }

            // Versions
            var previousVersionString = previousVersion == null ? null : _versionService.FormatVersion(previousVersion, configuration);
            var newVersionString = createLatest ? "latest" : _versionService.FormatVersion(project.ProjectProperties.DacVersion, configuration);

            // DACPAC paths
            var profilePath = Path.Combine(projectDirectory, configuration.PublishProfilePath);
            var artifactsPath = Path.Combine(projectDirectory, configuration.ArtifactsPath);
            var previousVersionDirectory = previousVersion == null ? null : Path.Combine(artifactsPath, previousVersionString);
            var previousVersionPath = previousVersion == null ? null : Path.Combine(previousVersionDirectory, $"{project.ProjectProperties.SqlTargetName}.dacpac");
            var newVersionDirectory = Path.Combine(artifactsPath, newVersionString);
            var newVersionPath = Path.Combine(newVersionDirectory, $"{project.ProjectProperties.SqlTargetName}.dacpac");
            var deployScriptPath = previousVersion == null
                                       ? null
                                       : Path.Combine(newVersionDirectory, $"{project.ProjectProperties.SqlTargetName}_{previousVersionString}_{newVersionString}.sql");
            var deployReportPath = previousVersion != null // Can only create report when comparing against a previous version
                                   && configuration.CreateDocumentationWithScriptCreation
                                       ? Path.Combine(newVersionDirectory, $"{project.ProjectProperties.SqlTargetName}_{previousVersionString}_{newVersionString}.xml")
                                       : null;

            return new PathCollection(profilePath,
                                      newVersionDirectory,
                                      newVersionPath,
                                      previousVersionPath,
                                      deployScriptPath,
                                      deployReportPath);
        }

        private async Task<bool> TryLoadSqlProjectPropertiesInternalAsync(SqlProject project)
        {
            var projectDirectory = Path.GetDirectoryName(project.FullName);
            if (projectDirectory == null)
            {
                await _logger.LogAsync($"ERROR: Cannot get project directory for {project.FullName}");
                return false;
            }

            var content = await _fileSystemAccess.ReadFileAsync(project.FullName);
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
                await _logger.LogAsync($"ERROR: Cannot read contents of {project.FullName} - {e.Message}");
                return false;
            }

            ReadProperties(root,
                           out var name,
                           out var outputPath,
                           out var sqlTargetName,
                           out var dacVersion);

            if (name == null)
            {
                await _logger.LogAsync($"ERROR: Cannot read name of {project.FullName}");
                return false;
            }

            if (outputPath == null)
            {
                await _logger.LogAsync($"ERROR: Cannot read output path of {project.FullName}");
                return false;
            }

            if (dacVersion == null)
            {
                await _logger.LogAsync($"ERROR: Cannot read DacVersion of {project.FullName}");
                return false;
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
}