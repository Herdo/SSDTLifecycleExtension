namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class SqlProjectTests
{
    [Test]
    public void Constructor_CorrectSettingOfProperties()
    {
        // Arrange
        const string name = "name";
        const string fullName = "fullName";
        var solutionItem = new object();

        // Act
        var p = new SqlProject(name, fullName, solutionItem);

        // Assert
        p.Name.Should().Be(name);
        p.FullName.Should().Be(fullName);
        p.SolutionItem.Should().BeSameAs(solutionItem);
        p.ProjectProperties.Should().NotBeNull();
        // Project properties should not be filled from within the constructor.
        p.ProjectProperties.SqlTargetName.Should().BeNull();
        p.ProjectProperties.BinaryDirectory.Should().BeNull();
        p.ProjectProperties.DacVersion.Should().BeNull();
    }
}
