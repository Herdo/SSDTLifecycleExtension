namespace SSDTLifecycleExtension.Shared.Contracts
{
    public class PathCollection
    {
        /// <summary>
        /// Gets the absolute path of the publish profile (xml) used to generate the deployment script and report.
        /// </summary>
        public string PublishProfilePath { get; }

        /// <summary>
        /// Gets the absolute directory of the new DACPAC.
        /// </summary>
        public string NewDacpacDirectory { get; }

        /// <summary>
        /// Gets the absolute path of the new DACPAC.
        /// </summary>
        public string NewDacpacPath { get; }

        /// <summary>
        /// Gets the absolute path of the previous DACPAC.
        /// </summary>
        public string PreviousDacpacPath { get; }

        /// <summary>
        /// Gets the absolute path of where to create the deployment script.
        /// </summary>
        public string DeployScriptPath { get; }

        /// <summary>
        /// Gets the absolute path of where to create the deployment report.
        /// </summary>
        public string DeployReportPath { get; }

        public PathCollection(string publishProfilePath,
                              string newDacpacDirectory,
                              string newDacpacPath,
                              string previousDacpacPath,
                              string deployScriptPath,
                              string deployReportPath)
        {
            PublishProfilePath = publishProfilePath;
            NewDacpacDirectory = newDacpacDirectory;
            NewDacpacPath = newDacpacPath;
            PreviousDacpacPath = previousDacpacPath;
            DeployScriptPath = deployScriptPath;
            DeployReportPath = deployReportPath;
        }
    }
}