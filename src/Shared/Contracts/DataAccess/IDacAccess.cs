namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess;

public interface IDacAccess
{
    Task<CreateDeployFilesResult> CreateDeployFilesAsync([NotNull] string previousVersionDacpacPath,
                                                         [NotNull] string newVersionDacpacPath,
                                                         [NotNull] string publishProfilePath,
                                                         bool createDeployScript,
                                                         bool createDeployReport);

    Task<(DefaultConstraint[] DefaultConstraints, string[] Errors)> GetDefaultConstraintsAsync([NotNull] string dacpacPath);
}