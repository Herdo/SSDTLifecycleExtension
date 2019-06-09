namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Services;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class SqlProjectService : ISqlProjectService
    {
        private readonly IFileSystemAccess _fileSystemAccess;

        public SqlProjectService(IFileSystemAccess fileSystemAccess)
        {
            _fileSystemAccess = fileSystemAccess;
        }

        async Task ISqlProjectService.LoadSqlProjectPropertiesAsync(SqlProject project)
        {
            var projectDirectory = Path.GetDirectoryName(project.FullName);
            if (projectDirectory == null)
                throw new InvalidOperationException("Cannot get project directory.");

            var content = await _fileSystemAccess.ReadFileAsync(project.FullName);
            var doc = XDocument.Parse(content);
            if (doc.Root == null)
                throw new InvalidOperationException($"Cannot read contents of {project.FullName}");

            string name = null;
            string outputPath = null;
            string sqlTargetName = null;

            var propertyGroups = doc.Root.Elements().Where(m => m.Name.LocalName == "PropertyGroup").ToArray();
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
            }

            if (name == null)
                throw new InvalidOperationException($"Cannot read name of {project.FullName}");
            if (outputPath == null)
                throw new InvalidOperationException($"Cannot read output path of {project.FullName}");

            // Set properties on the project object
            project.ProjectProperties.SqlTargetName = sqlTargetName ?? name;
            project.ProjectProperties.BinaryDirectory = Path.Combine(projectDirectory, outputPath);
        }
    }
}