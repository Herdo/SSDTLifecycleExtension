namespace SSDTLifecycleExtension.Shared.Contracts
{
    public class PathCollection
    {
        public string ProfilePath { get; }
        public string ArtifactsDirectoryWithVersion { get; }
        public string SourceFile { get; }
        public string TargetFile { get; }
        public string DeployScriptPath { get; }
        public string DeployReportPath { get; }

        public PathCollection(string profilePath,
                              string artifactsDirectoryWithVersion,
                              string sourceFile,
                              string targetFile,
                              string deployScriptPath,
                              string deployReportPath)
        {
            ProfilePath = profilePath;
            ArtifactsDirectoryWithVersion = artifactsDirectoryWithVersion;
            SourceFile = sourceFile;
            TargetFile = targetFile;
            DeployScriptPath = deployScriptPath;
            DeployReportPath = deployReportPath;
        }
    }
}