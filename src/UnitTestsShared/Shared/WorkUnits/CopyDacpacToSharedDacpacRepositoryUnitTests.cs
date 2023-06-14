namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class CopyDacpacToSharedDacpacRepositoryUnitTests
{
    [Test]
    public void Constructor_ArgumentNullException_FileSystemAccess()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new CopyDacpacToSharedDacpacRepositoryUnit(null, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Constructor_ArgumentNullException_Logger()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, null));
        // ReSharper restore AssignNullToNotNullAttribute
    }

    [Test]
    public void Work_ScaffoldingStateModel_ArgumentNullException_StateModel()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_NoRepositoryConfigured_Async()
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
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsNull(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_InvalidCharsInNewDacpacPath_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath" + new string(Path.GetInvalidPathChars()), "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsFalse(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.Is<string>(s => s.StartsWith("Failed to copy DACPAC to shared DACPAC repository: "))), Times.Once);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_FailedToEnsureTargetDirectoryExists_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
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
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"))
               .Returns("test directory error");
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsFalse(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to ensure that the directory 'C:\\Temp\\Test\\' exists: test directory error"), Times.Once);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_FailedToCopyFile_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
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
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"))
               .Returns(null as string);
        fsaMock.Setup(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"))
               .Returns("test copy error");
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsFalse(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to copy DACPAC to shared DACPAC repository: test copy error"), Times.Once);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_NoError_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
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
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"))
               .Returns(null as string);
        fsaMock.Setup(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"))
               .Returns(null as string);
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsNull(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
    {
        // Arrange
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_NoRepositoryConfigured_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 2, 3);
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
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsNull(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_InvalidCharsInNewDacpacPath_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath" + new string(Path.GetInvalidPathChars()), "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsFalse(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.Is<string>(s => s.StartsWith("Failed to copy DACPAC to shared DACPAC repository: "))), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_FailedToEnsureTargetDirectoryExists_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
        var previousVersion = new Version(1, 2, 3);
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
        fsaMock.Setup(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"))
               .Returns("test directory error");
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsFalse(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to ensure that the directory 'C:\\Temp\\Test\\' exists: test directory error"), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_FailedToCopyFile_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
        var previousVersion = new Version(1, 2, 3);
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
        fsaMock.Setup(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"))
               .Returns(null as string);
        fsaMock.Setup(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"))
               .Returns("test copy error");
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsFalse(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to copy DACPAC to shared DACPAC repository: test copy error"), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_NoError_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPath = "C:\\Temp\\Test\\";
        var previousVersion = new Version(1, 2, 3);
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
        fsaMock.Setup(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"))
               .Returns(null as string);
        fsaMock.Setup(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"))
               .Returns(null as string);
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoryUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        Assert.AreEqual(StateModelState.TriedToCopyDacpacToSharedDacpacRepository, model.CurrentState);
        Assert.IsNull(model.Result);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repository ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }
}