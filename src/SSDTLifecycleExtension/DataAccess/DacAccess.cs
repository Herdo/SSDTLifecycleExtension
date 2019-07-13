namespace SSDTLifecycleExtension.DataAccess
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.SqlServer.Dac;
    using Microsoft.SqlServer.Dac.Model;
    using Shared.Contracts;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using DefaultConstraint = Shared.Contracts.DefaultConstraint;

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

        private async Task<CreateDeployFilesResult> CreateDeployFilesInternalAsync(string previousVersionDacpacPath,
                                                                                   string newVersionDacpacPath,
                                                                                   string publishProfilePath,
                                                                                   bool createDeployScript,
                                                                                   bool createDeployReport)
        {
            return
                await Task.Run(() =>
                {
                    PublishResult result;
                    string preDeploymentScriptContent;
                    string postDeploymentScriptContent;
                    SecureStreamResult<DacPackage> previousDacpac = null;
                    SecureStreamResult<DacPackage> newDacpac = null;
                    SecureStreamResult<DacDeployOptions> deployOptions = null;
                    try
                    {
                        using (previousDacpac = _fileSystemAccess.ReadFromStream(previousVersionDacpacPath,
                                                                                 stream => DacPackage.Load(stream, DacSchemaModelStorageType.Memory)))
                            using (newDacpac = _fileSystemAccess.ReadFromStream(newVersionDacpacPath,
                                                                                stream => DacPackage.Load(stream, DacSchemaModelStorageType.Memory)))
                                using (deployOptions = _fileSystemAccess.ReadFromStream(publishProfilePath,
                                                                                        stream => DacProfile.Load(stream).DeployOptions))
                                {
                                    // Check for errors before processing
                                    var fileOpenErrors = new List<string>();
                                    if (previousDacpac.Exception != null)
                                        fileOpenErrors.Add($"Error reading previous DACPAC: {previousDacpac.Exception.Message}");
                                    if (newDacpac.Exception != null)
                                        fileOpenErrors.Add($"Error reading new DACPAC: {newDacpac.Exception.Message}");
                                    if (deployOptions.Exception != null)
                                        fileOpenErrors.Add($"Error reading publish profile: {deployOptions.Exception.Message}");
                                    if (fileOpenErrors.Any()
                                        || previousDacpac.Result == null
                                        || newDacpac.Result == null
                                        || deployOptions.Result == null)
                                        return new CreateDeployFilesResult(fileOpenErrors.ToArray());

                                    // Read pre-deployment and post-deployment from new DACPAC.
                                    preDeploymentScriptContent = TryToReadDeploymentScriptContent(newDacpac.Result.PreDeploymentScript);
                                    postDeploymentScriptContent = TryToReadDeploymentScriptContent(newDacpac.Result.PostDeploymentScript);

                                    // Process the input
                                    result = DacServices.Script(newDacpac.Result,
                                                                previousDacpac.Result,
                                                                "PRODUCTION",
                                                                new PublishOptions
                                                                {
                                                                    GenerateDeploymentScript = createDeployScript,
                                                                    GenerateDeploymentReport = createDeployReport,
                                                                    DeployOptions = deployOptions.Result
                                                                });
                                }

                    }
                    catch (DacServicesException e)
                    {
                        return new CreateDeployFilesResult(new[] {e.GetBaseException().Message});
                    }
                    finally
                    {
                        previousDacpac?.Dispose();
                        newDacpac?.Dispose();
                        deployOptions?.Dispose();
                    }

                    return new CreateDeployFilesResult(result?.DatabaseScript, _xmlFormatService.FormatDeployReport(result?.DeploymentReport), preDeploymentScriptContent, postDeploymentScriptContent);
                });
        }

        private static string TryToReadDeploymentScriptContent(Stream stream)
        {
            if (stream == null)
                return null;

            using (stream)
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        private async Task<(DefaultConstraint[] DefaultConstraints, string[] Errors)> GetDefaultConstraintsInternalAsync(string dacpacPath)
        {
            var (constraints, errors) = await Task.Run<(DefaultConstraint[] Result, string[] Errors)>(() =>
            {
                SecureStreamResult<TSqlModel> sqlModel = null;
                try
                {
                    using (sqlModel = _fileSystemAccess.ReadFromStream(dacpacPath,
                                                                       stream => TSqlModel.LoadFromDacpac(stream, new ModelLoadOptions(DacSchemaModelStorageType.Memory, false))))
                    {
                        // Check for errors before processing
                        var fileOpenErrors = new List<string>();
                        if (sqlModel.Exception != null)
                            fileOpenErrors.Add($"Error reading DACPAC: {sqlModel.Exception.Message}");
                        if (fileOpenErrors.Any() || sqlModel.Result == null)
                            return (null, fileOpenErrors.ToArray());

                        // Process the input
                        return (GetDefaultConstraintsFromModel(sqlModel.Result), null);
                    }

                }
                catch (Exception e) when (e is DacServicesException || e is DacModelException)
                {
                    return (null, new[] { e.GetBaseException().Message });
                }
                finally
                {
                    sqlModel?.Dispose();
                }
            });

            return (constraints, errors);
        }

        private static DefaultConstraint[] GetDefaultConstraintsFromModel(TSqlModel sqlModel)
        {
            var defaultConstraints = sqlModel.GetObjects(DacQueryScopes.UserDefined, ModelSchema.DefaultConstraint);
            var result = new List<DefaultConstraint>();
            foreach (var defaultConstraint in defaultConstraints)
            {
                var targetColumn = defaultConstraint.GetReferenced(Microsoft.SqlServer.Dac.Model.DefaultConstraint.TargetColumn)?.SingleOrDefault();
                if (targetColumn != null && targetColumn.Name.Parts.Count == 3)
                    result.Add(new DefaultConstraint(targetColumn.Name.Parts[0],
                                                     targetColumn.Name.Parts[1],
                                                     targetColumn.Name.Parts[2],
                                                     defaultConstraint.Name.HasName
                                                         ? defaultConstraint.Name.Parts[1] // Part[0] is the schema of the constraint
                                                         : null));
            }

            return result.ToArray();
        }

        Task<CreateDeployFilesResult> IDacAccess.CreateDeployFilesAsync(string previousVersionDacpacPath,
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

        Task<(DefaultConstraint[] DefaultConstraints, string[] Errors)> IDacAccess.GetDefaultConstraintsAsync(string dacpacPath)
        {
            if (dacpacPath == null)
                throw new ArgumentNullException(nameof(dacpacPath));

            return GetDefaultConstraintsInternalAsync(dacpacPath);
        }
    }
}