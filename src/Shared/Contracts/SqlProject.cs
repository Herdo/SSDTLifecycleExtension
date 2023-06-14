namespace SSDTLifecycleExtension.Shared.Contracts;

public class SqlProject
{
    [NotNull] public string Name { get; }

    [NotNull] public string FullName { get; }

    [NotNull] public string UniqueName { get; }

    [NotNull] public SqlProjectProperties ProjectProperties { get; }

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