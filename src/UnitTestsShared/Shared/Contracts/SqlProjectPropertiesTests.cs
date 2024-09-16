namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class SqlProjectPropertiesTests
{
    [Test]
    public void Default_AllPropertiesNull()
    {
        // Act
        var p = new SqlProjectProperties();

        // Assert
        p.SqlTargetName.Should().BeNull();
        p.BinaryDirectory.Should().BeNull();
        p.DacVersion.Should().BeNull();
    }

    [Test]
    public void SqlTargetName_SetGet()
    {
        // Arrange
        var p = new SqlProjectProperties();
        const string sqlTargetName = "sqlTarget";

        // Act
        p.SqlTargetName = sqlTargetName;

        // Assert
        p.SqlTargetName.Should().Be(sqlTargetName);
    }

    [Test]
    public void BinaryDirectory_SetGet()
    {
        // Arrange
        var p = new SqlProjectProperties();
        const string binaryDirectoryName = "binaryDirectory";

        // Act
        p.BinaryDirectory = binaryDirectoryName;

        // Assert
        p.BinaryDirectory.Should().Be(binaryDirectoryName);
    }

    [Test]
    public void DacVersion_SetGet()
    {
        // Arrange
        var p = new SqlProjectProperties();
        var v = new Version(2, 3, 0, 1);

        // Act
        p.DacVersion = v;

        // Assert
        p.DacVersion.Should().BeSameAs(v);
    }
}
