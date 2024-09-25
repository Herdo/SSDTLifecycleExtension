namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class CopyBuildResultUnitTests
{
    [Test]
    public async Task Work_ScaffoldingStateModel_CopiedSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.Directories.NewArtifactsDirectory)).ReturnsAsync(true);
        IWorkUnit<ScaffoldingStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyBuildResult);
        model.Result.Should().BeNull();
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_CopyFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.Directories.NewArtifactsDirectory)).ReturnsAsync(false);
        IWorkUnit<ScaffoldingStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyBuildResult);
        model.Result.Should().BeFalse();
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_CopiedSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, targetVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.Directories.NewArtifactsDirectory)).ReturnsAsync(true);
        IWorkUnit<ScriptCreationStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyBuildResult);
        model.Result.Should().BeNull();
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_CopyFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, targetVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.Directories.NewArtifactsDirectory)).ReturnsAsync(false);
        IWorkUnit<ScriptCreationStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyBuildResult);
        model.Result.Should().BeFalse();
    }
}