namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class LoadPathsUnitTests
{
    [Test]
    public async Task Work_ScaffoldingStateModel_LoadedSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var spsMock = new Mock<ISqlProjectService>();
        spsMock.Setup(m => m.TryLoadPathsForScaffoldingAsync(project, configuration)).ReturnsAsync(paths);
        IWorkUnit<ScaffoldingStateModel> unit = new LoadPathsUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.PathsLoaded);
        model.Result.Should().BeNull();
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_LoadFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
        var spsMock = new Mock<ISqlProjectService>();
        spsMock.Setup(m => m.TryLoadPathsForScaffoldingAsync(project, configuration)).ReturnsAsync(null as PathCollection);
        IWorkUnit<ScaffoldingStateModel> unit = new LoadPathsUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.PathsLoaded);
        model.Result.Should().BeFalse();
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_LoadedSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var spsMock = new Mock<ISqlProjectService>();
        spsMock.Setup(m => m.TryLoadPathsForScriptCreationAsync(project, configuration, previousVersion, true)).ReturnsAsync(paths);
        IWorkUnit<ScriptCreationStateModel> unit = new LoadPathsUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.PathsLoaded);
        model.Result.Should().BeNull();
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_LoadFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
        var spsMock = new Mock<ISqlProjectService>();
        spsMock.Setup(m => m.TryLoadPathsForScriptCreationAsync(project, configuration, previousVersion, true)).ReturnsAsync(null as PathCollection);
        IWorkUnit<ScriptCreationStateModel> unit = new LoadPathsUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.PathsLoaded);
        model.Result.Should().BeFalse();
    }
}