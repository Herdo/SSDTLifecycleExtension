namespace SSDTLifecycleExtension.Services
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Annotations;
    using DataAccess;

    [UsedImplicitly]
    public class SqlProjectService : ISqlProjectService
    {
        private readonly IFileSystemAccess _fileSystemAccess;

        public SqlProjectService(IFileSystemAccess fileSystemAccess)
        {
            _fileSystemAccess = fileSystemAccess;
        }

        async Task<(string OutputPath, string SqlTargetName)> ISqlProjectService.GetSqlProjectInformationAsync(string projectPath)
        {
            var content = await _fileSystemAccess.ReadFileAsync(projectPath);
            var doc = XDocument.Parse(content);
            if (doc.Root == null)
                throw new InvalidOperationException($"Cannot read contents of {projectPath}");

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
                throw new InvalidOperationException($"Cannot read name of {projectPath}");
            if (outputPath == null)
                throw new InvalidOperationException($"Cannot read output path of {projectPath}");

            return sqlTargetName == null
                       ? (outputPath, name)
                       : (outputPath, sqlTargetName);
        }
    }
}