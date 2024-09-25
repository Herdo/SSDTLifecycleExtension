namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class LoadSqlProjectPropertiesUnitTests
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
        var spsMock = new Mock<ISqlProjectService>();
        spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(true);
        IWorkUnit<ScaffoldingStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.SqlProjectPropertiesLoaded);
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
        spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(false);
        IWorkUnit<ScaffoldingStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.SqlProjectPropertiesLoaded);
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
        var spsMock = new Mock<ISqlProjectService>();
        spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(true);
        IWorkUnit<ScriptCreationStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.SqlProjectPropertiesLoaded);
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
        spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(false);
        IWorkUnit<ScriptCreationStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.SqlProjectPropertiesLoaded);
        model.Result.Should().BeFalse();
    }
}