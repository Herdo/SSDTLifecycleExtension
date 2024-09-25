namespace SSDTLifecycleExtension.UnitTests.Shared.Models;

[TestFixture]
public class ScriptModificationModelTests
{
    [Test]
    public void Constructor_CorrectInitialization()
    {
        // Arrange
        var initialScript = "script";
        var project = new SqlProject("a", "b", "c");
        var configuration = new ConfigurationModel();
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var previousVersion = new Version(1, 2, 0);

        // Act
        var model = new ScriptModificationModel(initialScript, project, configuration, paths, previousVersion, true);

        // Assert
        model.CurrentScript.Should().Be(initialScript);
        model.Project.Should().BeSameAs(project);
        model.Configuration.Should().BeSameAs(configuration);
        model.Paths.Should().BeSameAs(paths);
        model.PreviousVersion.Should().BeSameAs(previousVersion);
        model.CreateLatest.Should().BeTrue();
    }

    [Test]
    public void CurrentScript_Set_ArgumentNullException_WhenSetTotNull()
    {
        // Arrange
        var initialScript = "script";
        var project = new SqlProject("a", "b", "c");
        var configuration = new ConfigurationModel();
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var previousVersion = new Version(1, 2, 0);
        var model = new ScriptModificationModel(initialScript, project, configuration, paths, previousVersion, true);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Action act = () => model.CurrentScript = null;
        act.Should().Throw<ArgumentNullException>();
    }
}
