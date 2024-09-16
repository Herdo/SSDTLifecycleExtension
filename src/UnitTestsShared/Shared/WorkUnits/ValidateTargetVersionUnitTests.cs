namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class ValidateTargetVersionUnitTests
{
    [Test]
    public async Task Work_ScaffoldingStateModel_ValidVersion_Async()
    {
        // Arrange
        var dacVersion = new Version(1, 2, 3);
        var formattedTargetVersion = new Version(1, 2, 3);
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = dacVersion;
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
        {
            FormattedTargetVersion = formattedTargetVersion
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new ValidateTargetVersionUnit(vsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionValidated);
        model.Result.Should().BeNull();
        vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [TestCase("1.2.3", TestName = "DACPAC version is less than formatted target version")]
    [TestCase("1.2.5", TestName = "DACPAC version is greater than formatted target version")]
    public async Task Work_ScaffoldingStateModel_InvalidVersion_Async(string dacVersionString)
    {
        // Arrange
        var dacVersion = Version.Parse(dacVersionString);
        var formattedTargetVersion = new Version(1, 2, 4);
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = dacVersion;
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
        {
            FormattedTargetVersion = formattedTargetVersion
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new ValidateTargetVersionUnit(vsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionValidated);
        model.Result.Should().BeFalse();
        vsaMock.Verify(m => m.ShowModalError("Please change the DAC version in the SQL project settings (see output window)."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_ValidVersion_SpecificVersion_Async()
    {
        // Arrange
        var formattedTargetVersion = new Version(1, 2, 4);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandleWorkInProgressChanged)
        {
            FormattedTargetVersion = formattedTargetVersion
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ValidateTargetVersionUnit(vsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionValidated);
        model.Result.Should().BeNull();
        vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_ValidVersion_Latest_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged)
        {
            FormattedTargetVersion = null
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ValidateTargetVersionUnit(vsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionValidated);
        model.Result.Should().BeNull();
        vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    [TestCase("1.2.3", TestName = "Previous version is equal to formatted target version")]
    [TestCase("1.2.4", TestName = "Previous version is greater than formatted target version")]
    public async Task Work_ScriptCreationStateModel_InvalidVersion_Async(string previousVersionString)
    {
        // Arrange
        var formattedTargetVersion = new Version(1, 2, 3);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = Version.Parse(previousVersionString);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandleWorkInProgressChanged)
        {
            FormattedTargetVersion = formattedTargetVersion
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ValidateTargetVersionUnit(vsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.FormattedTargetVersionValidated);
        model.Result.Should().BeFalse();
        vsaMock.Verify(m => m.ShowModalError("Please change the DAC version in the SQL project settings (see output window)."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Once);
    }
}