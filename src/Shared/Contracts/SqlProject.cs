namespace SSDTLifecycleExtension.Shared.Contracts;

public class SqlProject
{
    public string Name { get; }

    public string FullName { get; }

    public object SolutionItem { get; }

    public SqlProjectProperties ProjectProperties { get; }

    public SqlProject(string name,
                      string fullName,
                      object solutionItem)
    {
        Name = name;
        FullName = fullName;
        SolutionItem = solutionItem;
        ProjectProperties = new SqlProjectProperties();
    }
}