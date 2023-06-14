namespace SSDTLifecycleExtension.Shared.Contracts;

public class CreateDeployFilesResult
{
    public string DeployScriptContent { get; }

    public string DeployReportContent { get; }

    public string PreDeploymentScript { get; }

    public string PostDeploymentScript { get; }

    public PublishProfile UsedPublishProfile { get; }

    public string[] Errors { get; }

    public CreateDeployFilesResult([CanBeNull] string deployScriptContent,
                                   [CanBeNull] string deployReportContent,
                                   [CanBeNull] string preDeploymentScript,
                                   [CanBeNull] string postDeploymentScript,
                                   [CanBeNull] PublishProfile usedPublishProfile)
    {
        DeployScriptContent = deployScriptContent;
        DeployReportContent = deployReportContent;
        PreDeploymentScript = preDeploymentScript;
        PostDeploymentScript = postDeploymentScript;
        UsedPublishProfile = usedPublishProfile;
    }

    public CreateDeployFilesResult([NotNull] string[] errors)
    {
        Errors = errors ?? throw new ArgumentNullException(nameof(errors));
    }
}