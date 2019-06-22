namespace SSDTLifecycleExtension.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.SqlServer.Dac;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;

    [UsedImplicitly]
    public class DacAccess : IDacAccess
    {
        private readonly IXmlFormatService _xmlFormatService;
        private readonly IFileSystemAccess _fileSystemAccess;

        public DacAccess([NotNull] IXmlFormatService xmlFormatService,
                         [NotNull] IFileSystemAccess fileSystemAccess)
        {
            _xmlFormatService = xmlFormatService ?? throw new ArgumentNullException(nameof(xmlFormatService));
            _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
        }

        [ExcludeFromCodeCoverage] // We're not testing the DacServices and other components in that namespace.
        private async Task<(string DeployScriptContent, string DeployReportContent, string[] Errors)> CreateDeployFilesInternalAsync(string previousVersionDacpacPath,
                                                                                                                                     string newVersionDacpacPath,
                                                                                                                                     string publishProfilePath,
                                                                                                                                     bool createDeployScript,
                                                                                                                                     bool createDeployReport)
        {
            var (publishResult, errors) = await Task.Run<(PublishResult Result, string[] Errors)>(() =>
            {
                PublishResult result;
                try
                {
                    var previousDacpac = _fileSystemAccess.ReadFromStream(previousVersionDacpacPath, 
                                                                          stream => DacPackage.Load(stream, DacSchemaModelStorageType.Memory));
                    var newDacpac = _fileSystemAccess.ReadFromStream(newVersionDacpacPath, 
                                                                     stream => DacPackage.Load(stream, DacSchemaModelStorageType.Memory));
                    var deployOptions = _fileSystemAccess.ReadFromStream(publishProfilePath, 
                                                                         stream => DacProfile.Load(stream).DeployOptions);

                    // Check for errors before processing
                    var fileOpenErrors = new List<string>();
                    if (previousDacpac.Exception != null)
                        fileOpenErrors.Add($"Error reading previous DACPAC: {previousDacpac.Exception.Message}");
                    if (newDacpac.Exception != null)
                        fileOpenErrors.Add($"Error reading new DACPAC: {newDacpac.Exception.Message}");
                    if (deployOptions.Exception != null)
                        fileOpenErrors.Add($"Error reading publish profile: {deployOptions.Exception.Message}");
                    if (fileOpenErrors.Any())
                        return (null, fileOpenErrors.ToArray());

                    // Process the input
                    result = DacServices.Script(newDacpac.Value,
                                                previousDacpac.Value,
                                                "PRODUCTION",
                                                new PublishOptions
                                                {
                                                    GenerateDeploymentScript = createDeployScript,
                                                    GenerateDeploymentReport = createDeployReport,
                                                    DeployOptions = deployOptions.Value
                                                });
                }
                catch (DacServicesException e)
                {
                    return (null, new[] {e.Message});
                }

                return (result, null);
            });

            return (publishResult?.DatabaseScript, _xmlFormatService.FormatDeployReport(publishResult?.DeploymentReport), errors);
        }

        Task<(string DeployScriptContent, string DeployReportContent, string[] Errors)> IDacAccess.CreateDeployFilesAsync(string previousVersionDacpacPath,
                                                                                                                          string newVersionDacpacPath,
                                                                                                                          string publishProfilePath,
                                                                                                                          bool createDeployScript,
                                                                                                                          bool createDeployReport)
        {
            if (previousVersionDacpacPath == null)
                throw new ArgumentNullException(nameof(previousVersionDacpacPath));
            if (newVersionDacpacPath == null)
                throw new ArgumentNullException(nameof(newVersionDacpacPath));
            if (publishProfilePath == null)
                throw new ArgumentNullException(nameof(publishProfilePath));
            if (!createDeployScript && !createDeployReport)
                throw new InvalidOperationException($"Either {nameof(createDeployScript)} or {nameof(createDeployReport)} must be true.");

            return CreateDeployFilesInternalAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, createDeployScript, createDeployReport);
        }
    }
}