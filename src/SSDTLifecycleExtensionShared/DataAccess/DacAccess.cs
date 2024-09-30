#nullable enable

namespace SSDTLifecycleExtension.DataAccess;

public class DacAccess(IXmlFormatService _xmlFormatService)
    : IDacAccess
{
    private async Task<CreateDeployFilesResult> CreateDeployFilesInternalAsync(string previousVersionDacpacPath,
        string newVersionDacpacPath,
        string publishProfilePath,
        bool createDeployScript,
        bool createDeployReport)
    {
        return
            await Task.Run(() =>
            {
                PublishResult? result;
                PublishProfile publishProfile;
                string? preDeploymentScriptContent;
                string? postDeploymentScriptContent;
                try
                {
                    // Get publish profile
                    var deployOptions = DacProfile.Load(publishProfilePath).DeployOptions;
                    publishProfile = ConvertPublishProfile(deployOptions);

                    // Read the DACPACs
                    using var previousDacpac = DacPackage.Load(previousVersionDacpacPath, DacSchemaModelStorageType.Memory);
                    using var newDacpac = DacPackage.Load(newVersionDacpacPath, DacSchemaModelStorageType.Memory);
                    // Read pre-deployment and post-deployment from new DACPAC.
                    preDeploymentScriptContent = TryToReadDeploymentScriptContent(newDacpac.PreDeploymentScript);
                    postDeploymentScriptContent = TryToReadDeploymentScriptContent(newDacpac.PostDeploymentScript);

                    // Process the input
                    result = DacServices.Script(newDacpac,
                        previousDacpac,
                        "PRODUCTION",
                        new PublishOptions
                        {
                            GenerateDeploymentScript = createDeployScript,
                            GenerateDeploymentReport = createDeployReport,
                            DeployOptions = deployOptions
                        });

                    if (result?.DeploymentReport is null)
                        return new CreateDeployFilesResult(["Created deployment script is null."]);
                }
                catch (DacServicesException e)
                {
                    return new CreateDeployFilesResult(GetErrorList(e));
                }

                return new CreateDeployFilesResult(result.DatabaseScript,
                    _xmlFormatService.FormatDeployReport(result.DeploymentReport),
                    preDeploymentScriptContent,
                    postDeploymentScriptContent,
                    publishProfile);
            });
    }

    private static PublishProfile ConvertPublishProfile(DacDeployOptions deployOptions)
    {
        return new PublishProfile
        {
            CreateNewDatabase = deployOptions.CreateNewDatabase,
            BackupDatabaseBeforeChanges = deployOptions.BackupDatabaseBeforeChanges,
            ScriptDatabaseOptions = deployOptions.ScriptDatabaseOptions,
            ScriptDeployStateChecks = deployOptions.ScriptDeployStateChecks
        };
    }

    private static string? TryToReadDeploymentScriptContent(Stream stream)
    {
        if (stream is null)
            return null;

        using (stream)
        {
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
    }

    private async Task<(DefaultConstraint[]? DefaultConstraints, string[]? Errors)> GetDefaultConstraintsInternalAsync(string dacpacPath)
    {
        var (constraints, errors) = await Task.Run<(DefaultConstraint[]? Result, string[]? Errors)>(() =>
        {
            try
            {
                using var sqlModel = TSqlModel.LoadFromDacpac(dacpacPath, new ModelLoadOptions(DacSchemaModelStorageType.Memory, false));
                // Process the input
                return (GetDefaultConstraintsFromModel(sqlModel), null);
            }
            catch (DacModelException e)
            {
                var errorMessages = new List<string> { e.GetBaseException().Message };
                if (e.Messages is not null && e.Messages.Any())
                    errorMessages.AddRange(e.Messages.Select(m => m.ToString()));
                return (null, [.. errorMessages]);
            }
            catch (Exception e)
            {
                return (null, [e.GetBaseException().Message]);
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
            if (targetColumn is not null && targetColumn.Name.Parts.Count == 3)
                result.Add(new DefaultConstraint(targetColumn.Name.Parts[0],
                    targetColumn.Name.Parts[1],
                    targetColumn.Name.Parts[2],
                    defaultConstraint.Name.HasName
                        ? defaultConstraint.Name.Parts[1] // Part[0] is the schema of the constraint
                        : null));
        }

        return [.. result];
    }

    private static string[] GetErrorList(DacServicesException e)
    {
        var errorList = new List<string>
        {
            e.GetBaseException().Message
        };
        errorList.AddRange(e.Messages.Select(dacMessage => dacMessage.ToString()));
        return [.. errorList];
    }

    Task<CreateDeployFilesResult> IDacAccess.CreateDeployFilesAsync(string previousVersionDacpacPath,
        string newVersionDacpacPath,
        string publishProfilePath,
        bool createDeployScript,
        bool createDeployReport)
    {
        if (!createDeployScript && !createDeployReport)
            throw new InvalidOperationException($"Either {nameof(createDeployScript)} or {nameof(createDeployReport)} must be true.");

        return CreateDeployFilesInternalAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, createDeployScript, createDeployReport);
    }

    Task<(DefaultConstraint[]? DefaultConstraints, string[]? Errors)> IDacAccess.GetDefaultConstraintsAsync(string dacpacPath)
    {
        return GetDefaultConstraintsInternalAsync(dacpacPath);
    }
}