namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using JetBrains.Annotations;

    public class CreateDeployFilesResult
    {
        public string DeployScriptContent { get; }

        public string DeployReportContent { get; }

        public string PreDeploymentScript { get; }

        public string PostDeploymentScript { get; }

        public string[] Errors { get; }

        public CreateDeployFilesResult([CanBeNull] string deployScriptContent,
                                       [CanBeNull] string deployReportContent,
                                       [CanBeNull] string preDeploymentScript,
                                       [CanBeNull] string postDeploymentScript)
        {
            DeployScriptContent = deployScriptContent;
            DeployReportContent = deployReportContent;
            PreDeploymentScript = preDeploymentScript;
            PostDeploymentScript = postDeploymentScript;
        }

        public CreateDeployFilesResult([NotNull] string[] errors)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }
    }
}