namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class PublishProfileTests
{
    [Test]
    public void CreateNewDatabase_Get_Set()
    {
        // Arrange
        var instance = new PublishProfile
        {
            CreateNewDatabase = false
        };

        // Act
        instance.CreateNewDatabase = true;

        // Assert
        instance.CreateNewDatabase.Should().BeTrue();
    }

    [Test]
    public void BackupDatabaseBeforeChanges_Get_Set()
    {
        // Arrange
        var instance = new PublishProfile
        {
            BackupDatabaseBeforeChanges = false
        };

        // Act
        instance.BackupDatabaseBeforeChanges = true;

        // Assert
        instance.BackupDatabaseBeforeChanges.Should().BeTrue();
    }

    [Test]
    public void ScriptDatabaseOptions_Get_Set()
    {
        // Arrange
        var instance = new PublishProfile
        {
            ScriptDatabaseOptions = false
        };

        // Act
        instance.ScriptDatabaseOptions = true;

        // Assert
        instance.ScriptDatabaseOptions.Should().BeTrue();
    }

    [Test]
    public void ScriptDeployStateChecks_Get_Set()
    {
        // Arrange
        var instance = new PublishProfile
        {
            ScriptDeployStateChecks = false
        };

        // Act
        instance.ScriptDeployStateChecks = true;

        // Assert
        instance.ScriptDeployStateChecks.Should().BeTrue();
    }
}
