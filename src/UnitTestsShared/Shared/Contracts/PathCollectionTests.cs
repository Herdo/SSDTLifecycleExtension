namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class PathCollectionTests
{
    [Test]
    public void Constructor_ArgumentNullException_Directories()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new PathCollection(null, null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_DeploySources()
    {
        // Arrange
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new PathCollection(directories, null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_DeployTargets()
    {
        // Arrange
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new PathCollection(directories, sourcePaths, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_InvalidOperationException_NeitherScriptPathNorDeployPathSet_WhenPreviousDacpacPathIsSet()
    {
        // Arrange
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths(null, null);

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<InvalidOperationException>(() => new PathCollection(directories, sourcePaths, targetPaths));
    }

    [Test]
    public void Constructor_NoInvalidOperationException_NeitherScriptPathNorDeployPathSet_WhenPreviousDacpacPathIsNotSet()
    {
        // Arrange
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", null);
        var targetPaths = new DeployTargetPaths(null, null);

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.DoesNotThrow(() => new PathCollection(directories, sourcePaths, targetPaths));
    }

    [Test]
    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Constructor_CorrectSettingOfProperties(bool setDeployScriptPath, bool setDeployReportPath)
    {
        // Arrange
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths(setDeployScriptPath ? "deployScriptPath" : null,
                                                setDeployReportPath ? "deployReportPath" : null);

        // Act
        var pc = new PathCollection(directories,
                                    sourcePaths,
                                    targetPaths);

        // Assert
        Assert.AreSame(directories, pc.Directories);
        Assert.AreSame(sourcePaths, pc.DeploySources);
        Assert.AreSame(targetPaths, pc.DeployTargets);
    }
}