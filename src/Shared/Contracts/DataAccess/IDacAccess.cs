namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface IDacAccess
{
    Task<CreateDeployFilesResult> CreateDeployFilesAsync(string previousVersionDacpacPath,
        string newVersionDacpacPath,
        string publishProfilePath,
        bool createDeployScript,
        bool createDeployReport);

    Task<(DefaultConstraint[]? DefaultConstraints, string[]? Errors)> GetDefaultConstraintsAsync(string dacpacPath);
}