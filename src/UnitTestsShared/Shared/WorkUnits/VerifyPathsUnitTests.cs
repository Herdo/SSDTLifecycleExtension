﻿namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class VerifyPathsUnitTests
{
    [Test]
    public void Constructor_ArgumentNullException_FileSystemAccess()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new VerifyPathsUnit(null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_Logger()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new VerifyPathsUnit(fsaMock, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new VerifyPathsUnit(fsaMock, loggerMock);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_VerificationSuccessful_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.CheckIfFileExists(paths.DeploySources.PublishProfilePath)).Returns(true);
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new VerifyPathsUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.PathsVerified, model.CurrentState);
        Assert.IsNull(model.Result);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_VerificationFailed_PublishProfilePathIsNotFilled_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.SetupGet(m => m.DocumentationBaseUrl).Returns("foobasebar");
        IWorkUnit<ScriptCreationStateModel> unit = new VerifyPathsUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.PathsVerified, model.CurrentState);
        Assert.IsFalse(model.Result);
        loggerMock.Verify(m => m.LogErrorAsync(It.Is<string>(s => s.Contains(ConfigurationModel.UseSinglePublishProfileSpecialKeyword) && s.Contains("foobasebarpublish-profile-path"))), Times.Once);
        fsaMock.Verify(m => m.CheckIfFileExists(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_VerificationFailed_PublishProfileDoesNotExist_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.CheckIfFileExists(paths.DeploySources.PublishProfilePath)).Returns(false);
        var loggerMock = new Mock<ILogger>();
        loggerMock.SetupGet(m => m.DocumentationBaseUrl).Returns("foobasebar");
        IWorkUnit<ScriptCreationStateModel> unit = new VerifyPathsUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.PathsVerified, model.CurrentState);
        Assert.IsFalse(model.Result);
        loggerMock.Verify(m => m.LogErrorAsync(It.Is<string>(s => s.Contains("publishProfilePath") && s.Contains("foobasebarpublish-profile-path"))), Times.Once);
    }
}