namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class CleanNewArtifactsDirectoryUnitTests
{
    [Test]
    public async Task Work_ScaffoldingStateModel_CompleteRun_Async()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CleanNewArtifactsDirectoryUnit(fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            Paths = paths
        };

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCleanArtifactsDirectory);
        model.Result.Should().BeNull();
        fsaMock.Verify(m => m.TryToCleanDirectory("newArtifactsDirectory"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsNotNull<string>()), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_CompleteRun_Async()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CleanNewArtifactsDirectoryUnit(fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandlerFunc)
        {
            Paths = paths
        };

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCleanArtifactsDirectory);
        model.Result.Should().BeNull();
        fsaMock.Verify(m => m.TryToCleanDirectory("newArtifactsDirectory"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsNotNull<string>()), Times.Once);
    }
}