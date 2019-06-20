namespace SSDTLifecycleExtension.DataAccess
{
    using System;
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

        public DacAccess([NotNull] IXmlFormatService xmlFormatService)
        {
            _xmlFormatService = xmlFormatService ?? throw new ArgumentNullException(nameof(xmlFormatService));
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
                    var previousDacpac = DacPackage.Load(previousVersionDacpacPath, DacSchemaModelStorageType.Memory);
                    var newDacpac = DacPackage.Load(newVersionDacpacPath, DacSchemaModelStorageType.Memory);
                    var deployOptions = DacProfile.Load(publishProfilePath).DeployOptions;
                    result = DacServices.Script(newDacpac,
                                                previousDacpac,
                                                "PRODUCTION",
                                                new PublishOptions
                                                {
                                                    GenerateDeploymentScript = createDeployScript,
                                                    GenerateDeploymentReport = createDeployReport,
                                                    DeployOptions = deployOptions
                                                });
                }
                catch (DacServicesException e)
                {
                    if (e.Messages.Any())
                    {
                        return (null, e.Messages
                                       .Select(m => m.ToString())
                                       .ToArray());
                    }

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