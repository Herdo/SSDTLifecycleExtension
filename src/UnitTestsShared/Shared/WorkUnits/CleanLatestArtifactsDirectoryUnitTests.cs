namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class CleanLatestArtifactsDirectoryUnitTests
{
    [Test]
    public async Task Work_ScriptCreationStateModel_CompleteRun_NoDeletion_ConfigurationDisabled_Async()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CleanLatestArtifactsDirectoryUnit(fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.DeleteLatestAfterVersionedScriptGeneration = false;
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
        model.CurrentState.Should().Be(StateModelState.DeletedLatestArtifacts);
        model.Result.Should().BeNull();
        fsaMock.Verify(m => m.TryToCleanDirectory(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.TryToCleanDirectory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_CompleteRun_Async()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CleanLatestArtifactsDirectoryUnit(fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.DeleteLatestAfterVersionedScriptGeneration = true;
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
        model.CurrentState.Should().Be(StateModelState.DeletedLatestArtifacts);
        model.Result.Should().BeNull();
        fsaMock.Verify(m => m.TryToCleanDirectory("latestArtifactsDirectory"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsNotNull<string>()), Times.Once);
    }
}