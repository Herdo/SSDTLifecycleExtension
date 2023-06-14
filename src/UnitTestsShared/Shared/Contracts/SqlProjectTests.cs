namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class SqlProjectTests
{
    [Test]
    public void Constructor_ArgumentNullException_Name()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new SqlProject(null, null, null));
    }

    [Test]
    public void Constructor_ArgumentNullException_FullName()
    {
        // Arrange
        const string name = "name";

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new SqlProject(name, null, null));
    }

    [Test]
    public void Constructor_ArgumentNullException_UniqueName()
    {
        // Arrange
        const string name = "name";
        const string fullName = "fullName";

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new SqlProject(name, fullName, null));
    }

    [Test]
    public void Constructor_CorrectSettingOfProperties()
    {
        // Arrange
        const string name = "name";
        const string fullName = "fullName";
        const string uniqueName = "uniqueName";

        // Act
        var p = new SqlProject(name, fullName, uniqueName);

        // Assert
        Assert.AreEqual(name, p.Name);
        Assert.AreEqual(fullName, p.FullName);
        Assert.AreEqual(uniqueName, p.UniqueName);
        Assert.IsNotNull(p.ProjectProperties);
        // Project properties should not be filled from within the constructor.
        Assert.IsNull(p.ProjectProperties.SqlTargetName);
        Assert.IsNull(p.ProjectProperties.BinaryDirectory);
        Assert.IsNull(p.ProjectProperties.DacVersion);
    }
}