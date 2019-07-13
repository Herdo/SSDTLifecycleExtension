namespace SSDTLifecycleExtension.Shared.Contracts
{
    public class PublishProfile
    {
        public bool CreateNewDatabase { get; set; }

        public bool BackupDatabaseBeforeChanges { get; set; }

        public bool ScriptDatabaseOptions { get; set; }

        public bool ScriptDeployStateChecks { get; set; }
    }
}