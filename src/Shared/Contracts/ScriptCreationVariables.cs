namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;

    public class ScriptCreationVariables
    {
        public string SqlTargetName { get; }
        public string ProjectPath { get; }
        public string BinaryDirectory { get; }
        public string ProfilePath { get; }
        public string SourceDirectory { get; }
        public string SourceFile { get; }
        public string TargetFile { get; }
        public string DeployScriptPath { get; }
        public string DeployReportPath { get; }
        public bool CreateDocumentation { get; }
        public Version PreviousVersion { get; }
        public Version NewVersion { get; }

        public ScriptCreationVariables(string sqlTargetName,
                                       string projectPath,
                                       string binaryDirectory,
                                       string profilePath,
                                       string sourceDirectory,
                                       string sourceFile,
                                       string targetFile,
                                       string deployScriptPath,
                                       string deployReportPath,
                                       Version previousVersion,
                                       Version newVersion)
        {
            SqlTargetName = sqlTargetName;
            ProjectPath = projectPath;
            BinaryDirectory = binaryDirectory;
            ProfilePath = profilePath;
            SourceDirectory = sourceDirectory;
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