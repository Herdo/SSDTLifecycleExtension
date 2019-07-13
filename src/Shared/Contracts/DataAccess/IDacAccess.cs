namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IDacAccess
    {
        Task<CreateDeployFilesResult> CreateDeployFilesAsync([NotNull] string previousVersionDacpacPath,
                                                             [NotNull] string newVersionDacpacPath,
                                                             [NotNull] string publishProfilePath,
                                                             bool createDeployScript,
                                                             bool createDeployReport,
                                                             [NotNull] Func<PublishProfile, Task<bool>> validatePublishProfile);

        Task<(DefaultConstraint[] DefaultConstraints, string[] Errors)> GetDefaultConstraintsAsync([NotNull] string dacpacPath);
    }
}