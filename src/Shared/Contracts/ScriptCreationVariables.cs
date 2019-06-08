namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;

    public class ScriptCreationVariables
    {
        public string ProfilePath { get; }
        public string ArtifactsDirectoryWithVersion { get; }
        public string SourceFile { get; }
        public string TargetFile { get; }
        public string DeployScriptPath { get; }
        public string DeployReportPath { get; }
        public bool CreateDocumentation { get; }
        public Version PreviousVersion { get; }
        public Version NewVersion { get; }

        public ScriptCreationVariables(string profilePath,
                                       string artifactsDirectoryWithVersion,
                                       string sourceFile,
                                       string targetFile,
                                       string deployScriptPath,
                                       string deployReportPath,
                                       Version previousVersion,
                                       Version newVersion)
        {
            ProfilePath = profilePath;
            ArtifactsDirectoryWithVersion = artifactsDirectoryWithVersion;
            SourceFile = sourceFile;
            TargetFile = targetFile;
            DeployScriptPath = deployScriptPath;
            DeployReportPath = deployReportPath;
            PreviousVersion = previousVersion;
            NewVersion = newVersion;
            CreateDocumentation = deployReportPath != null;
        }
    }
}