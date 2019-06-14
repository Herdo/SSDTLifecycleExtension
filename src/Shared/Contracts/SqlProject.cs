namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;

    public class SqlProject
    {
        public string Name { get; }

        public string FullName { get; }

        public string UniqueName { get; }

        public SqlProjectProperties ProjectProperties { get; }

        public SqlProject(string name,
                          string fullName,
                          string uniqueName)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            UniqueName = uniqueName ?? throw new ArgumentNullException(nameof(uniqueName));
            ProjectProperties = new SqlProjectProperties();
        }
    }
}