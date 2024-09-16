namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class FormatTargetVersionUnitTests
{
    [Test]
    public async Task Work_ScaffoldingStateModel_FormattedSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        var expectedFormattedTargetVersion = new Version(1, 2, 3, 0);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(targetVersion, configuration)).Returns(expectedFormattedTargetVersion.ToString());
        IWorkUnit<ScaffoldingStateModel> unit = new FormatTargetVersionUnit(vsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionLoaded);
        model.Result.Should().BeNull();
        model.FormattedTargetVersion.Should().Be(expectedFormattedTargetVersion);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task Work_ScaffoldingStateModel_FormattedSuccessful_Async(bool latest)
    {
        // Arrange
        var dacVersion = new Version(1, 2, 3);
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = dacVersion;
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var expectedFormattedTargetVersion = new Version(1, 2, 3, 0);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, latest, HandleWorkInProgressChanged);
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(dacVersion, configuration)).Returns(expectedFormattedTargetVersion.ToString());
        IWorkUnit<ScriptCreationStateModel> unit = new FormatTargetVersionUnit(vsMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionLoaded);
        model.Result.Should().BeNull();
        if (latest)
        {
            model.FormattedTargetVersion.Should().BeNull();
        }
        else
        {
            model.FormattedTargetVersion.Should().Be(expectedFormattedTargetVersion);
        }
    }
}