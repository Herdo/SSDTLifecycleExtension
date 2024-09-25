namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class DirectoryPathsTests
{
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
        dp.ProjectDirectory.Should().Be(projectDirectory);
        dp.LatestArtifactsDirectory.Should().Be(latestArtifactsDirectory);
        dp.NewArtifactsDirectory.Should().Be(newArtifactsDirectory);
    }
}