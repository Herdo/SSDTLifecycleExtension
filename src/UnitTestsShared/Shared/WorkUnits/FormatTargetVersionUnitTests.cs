namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class FormatTargetVersionUnitTests
{
    [Test]
    public void Constructor_ArgumentNullException_VersionService()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new FormatTargetVersionUnit(null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Work_ScaffoldingStateModel_ArgumentNullException_StateModel()
    {
        // Arrange
        var vsMock = new Mock<IVersionService>();
        IWorkUnit<ScaffoldingStateModel> unit = new FormatTargetVersionUnit(vsMock.Object);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
    }

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
        Assert.AreEqual(StateModelState.FormattedTargetVersionLoaded, model.CurrentState);
        Assert.IsNull(model.Result);
        Assert.AreEqual(expectedFormattedTargetVersion, model.FormattedTargetVersion);
    }

    [Test]
    public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
    {
        // Arrange
        var vsMock = new Mock<IVersionService>();
        IWorkUnit<ScriptCreationStateModel> unit = new FormatTargetVersionUnit(vsMock.Object);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
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
        Assert.AreEqual(StateModelState.FormattedTargetVersionLoaded, model.CurrentState);
        Assert.IsNull(model.Result);
        if (latest)
        {
            Assert.IsNull(model.FormattedTargetVersion);
        }
        else
        {
            Assert.AreEqual(expectedFormattedTargetVersion, model.FormattedTargetVersion);
        }
    }
}