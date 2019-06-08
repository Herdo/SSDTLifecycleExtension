namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;

    public class SqlProjectProperties
    {
        public string SqlTargetName { get; set; }

        public string BinaryDirectory { get; set; }

        public Version ConfiguredVersion { get; set; }
    }
}