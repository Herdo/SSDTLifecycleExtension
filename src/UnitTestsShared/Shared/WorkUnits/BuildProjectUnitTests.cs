namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class BuildProjectUnitTests
{
    [Test]
    public async Task Work_ScaffoldingStateModel_BuiltSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(true);
        IWorkUnit<ScaffoldingStateModel> unit = new BuildProjectUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToBuildProject);
        model.Result.Should().BeNull();
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_BuildFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(false);
        IWorkUnit<ScaffoldingStateModel> unit = new BuildProjectUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToBuildProject);
        model.Result.Should().BeFalse();
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_SkipBuild_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.BuildBeforeScriptCreation = false;
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(true);
        IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToBuildProject);
        model.Result.Should().BeNull();
        bsMock.Verify(m => m.BuildProjectAsync(project), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_BuiltSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.BuildBeforeScriptCreation = true;
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(true);
        IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToBuildProject);
        model.Result.Should().BeNull();
        bsMock.Verify(m => m.BuildProjectAsync(project), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_BuildFailed_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
        var bsMock = new Mock<IBuildService>();
        bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(false);
        IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToBuildProject);
        model.Result.Should().BeFalse();
    }
}