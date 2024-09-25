namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class ModifyDeploymentScriptUnitTests
{
    [Test]
    public async Task Work_ScriptCreationStateModel_NoModifiers_Async()
    {
        // Arrange
        var mpsMock = new Mock<IScriptModifierProviderService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock.Object, fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            Paths = new PathCollection(
                new DirectoryPaths("a", "b", "c"),
                new DeploySourcePaths("a", "b", "c"),
                new DeployTargetPaths("a", "b"))
        };
        mpsMock.Setup(m => m.GetScriptModifiers(configuration)).Returns(new Dictionary<ScriptModifier, IScriptModifier>());

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.ModifiedDeploymentScript);
        model.Result.Should().BeNull();
        mpsMock.Verify(m => m.GetScriptModifiers(It.IsAny<ConfigurationModel>()), Times.Once);
        fsaMock.Verify(m => m.ReadFileAsync(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_FailedToReadGeneratedScript_Async()
    {
        // Arrange
        var exception = new IOException("foo");
        const string expectedResultScript = "foo bar ab";
        var smHeaderMock = new Mock<IScriptModifier>();
        var smFooterMock = new Mock<IScriptModifier>();
        var mpsMock = new Mock<IScriptModifierProviderService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync("deployScriptPath"))
               .Throws(exception);
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock.Object, fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        const bool createLatest = false;
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, createLatest, HandlerFunc)
        {
            Paths = paths
        };
        mpsMock.Setup(m => m.GetScriptModifiers(configuration)).Returns(new Dictionary<ScriptModifier, IScriptModifier>
        {
            {ScriptModifier.AddCustomFooter, smFooterMock.Object},
            {ScriptModifier.AddCustomHeader, smHeaderMock.Object}
        });
        smHeaderMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                    .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += " a")
                    .Returns(Task.CompletedTask);
        smFooterMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                    .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += "b")
                    .Returns(Task.CompletedTask);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.ModifiedDeploymentScript);
        model.Result.Should().BeFalse();
        mpsMock.Verify(m => m.GetScriptModifiers(It.IsAny<ConfigurationModel>()), Times.Once);
        fsaMock.Verify(m => m.ReadFileAsync(paths.DeployTargets.DeployScriptPath), Times.Once);
        fsaMock.Verify(m => m.WriteFileAsync(paths.DeployTargets.DeployScriptPath, expectedResultScript), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Never);
        smHeaderMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Never);
        smFooterMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(exception, "Failed to read the generated script"), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_FailedToWriteModifiedScript_Async()
    {
        // Arrange
        var exception = new IOException("foo");
        const string baseScript = "foo bar";
        const string expectedResultScript = "foo bar ab";
        var smHeaderMock = new Mock<IScriptModifier>();
        var smFooterMock = new Mock<IScriptModifier>();
        var mpsMock = new Mock<IScriptModifierProviderService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync("deployScriptPath"))
               .ReturnsAsync(baseScript);
        fsaMock.Setup(m => m.WriteFileAsync("deployScriptPath", It.IsAny<string>()))
               .Throws(exception);
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock.Object, fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        const bool createLatest = false;
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, createLatest, HandlerFunc)
        {
            Paths = paths
        };
        mpsMock.Setup(m => m.GetScriptModifiers(configuration)).Returns(new Dictionary<ScriptModifier, IScriptModifier>
        {
            {ScriptModifier.AddCustomFooter, smFooterMock.Object},
            {ScriptModifier.AddCustomHeader, smHeaderMock.Object}
        });
        smHeaderMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                    .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += " a")
                    .Returns(Task.CompletedTask);
        smFooterMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                    .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += "b")
                    .Returns(Task.CompletedTask);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.ModifiedDeploymentScript);
        model.Result.Should().BeFalse();
        mpsMock.Verify(m => m.GetScriptModifiers(It.IsAny<ConfigurationModel>()), Times.Once);
        fsaMock.Verify(m => m.ReadFileAsync(paths.DeployTargets.DeployScriptPath), Times.Once);
        fsaMock.Verify(m => m.WriteFileAsync(paths.DeployTargets.DeployScriptPath, expectedResultScript), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Exactly(2));
        smHeaderMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Once);
        smFooterMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(exception, "Failed to write the modified script"), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_CorrectModifierExecution_Async()
    {
        // Arrange
        const string baseScript = "foo bar";
        const string expectedResultScript = "foo bar ab";
        var smHeaderMock = new Mock<IScriptModifier>();
        var smFooterMock = new Mock<IScriptModifier>();
        var mpsMock = new Mock<IScriptModifierProviderService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync("deployScriptPath"))
               .ReturnsAsync(baseScript);
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock.Object, fsaMock.Object, loggerMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        const bool createLatest = false;
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, createLatest, HandlerFunc)
        {
            Paths = paths
        };
        mpsMock.Setup(m => m.GetScriptModifiers(configuration)).Returns(new Dictionary<ScriptModifier, IScriptModifier>
        {
            {ScriptModifier.AddCustomFooter, smFooterMock.Object},
            {ScriptModifier.AddCustomHeader, smHeaderMock.Object}
        });
        smHeaderMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                    .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += " a")
                    .Returns(Task.CompletedTask);
        smFooterMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                    .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += "b")
                    .Returns(Task.CompletedTask);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.ModifiedDeploymentScript);
        model.Result.Should().BeNull();
        mpsMock.Verify(m => m.GetScriptModifiers(It.IsAny<ConfigurationModel>()), Times.Once);
        fsaMock.Verify(m => m.ReadFileAsync(paths.DeployTargets.DeployScriptPath), Times.Once);
        fsaMock.Verify(m => m.WriteFileAsync(paths.DeployTargets.DeployScriptPath, expectedResultScript), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Exactly(2));
        smHeaderMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Once);
        smFooterMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Once);
    }
}