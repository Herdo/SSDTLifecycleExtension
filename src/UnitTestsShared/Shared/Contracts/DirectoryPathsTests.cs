namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class DirectoryPathsTests
{
    [Test]
    public void Constructor_ArgumentNullException_ProjectDirectory()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new DirectoryPaths(null, null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_LatestArtifactsDirectory()
    {
        // Arrange
        const string projectDirectory = "projectDirectory";

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new DirectoryPaths(projectDirectory, null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_NewArtifactsDirectory()
    {
        // Arrange
        const string projectDirectory = "projectDirectory";
        const string latestArtifactsDirectory = "latestArtifactsDirectory";

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new DirectoryPaths(projectDirectory, latestArtifactsDirectory, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_CorrectSettingOfProperties()
    {
        // Arrange
        const string projectDirectory = "projectDirectory";
        const string latestArtifactsDirectory = "latestArtifactsDirectory";
        const string newArtifactsDirectory = "newArtifactsDirectory";

        // Act
        var dp = new DirectoryPaths(projectDirectory, latestArtifactsDirectory, newArtifactsDirectory);

        // Assert
        Assert.AreEqual(projectDirectory, dp.ProjectDirectory);
        Assert.AreEqual(latestArtifactsDirectory, dp.LatestArtifactsDirectory);
        Assert.AreEqual(newArtifactsDirectory, dp.NewArtifactsDirectory);
    }
}