namespace SSDTLifecycleExtension.Shared.Contracts;

public class CreateDeployFilesResult
{
    public string? DeployScriptContent { get; }

    public string? DeployReportContent { get; }

    public string? PreDeploymentScript { get; }

    public string? PostDeploymentScript { get; }

    public PublishProfile? UsedPublishProfile { get; }

    public string[]? Errors { get; }

    public CreateDeployFilesResult(string? deployScriptContent,
        string? deployReportContent,
        string? preDeploymentScript,
        string? postDeploymentScript,
        PublishProfile usedPublishProfile)
    {
        DeployScriptContent = deployScriptContent;
        DeployReportContent = deployReportContent;
        PreDeploymentScript = preDeploymentScript;
        PostDeploymentScript = postDeploymentScript;
        UsedPublishProfile = usedPublishProfile;
    }

    public CreateDeployFilesResult(string[] errors)
    {
        Errors = errors;
    }
}