namespace SSDTLifecycleExtension.Shared.Contracts;

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
        Name = name;
        FullName = fullName;
        UniqueName = uniqueName;
        ProjectProperties = new SqlProjectProperties();
    }
}