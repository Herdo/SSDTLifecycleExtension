namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class CopyDacpacToSharedDacpacRepositoriesUnitTests
{
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
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeNull();
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
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\";
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
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeFalse();
        fsaMock.Verify(m => m.EnsureDirectoryExists(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.Is<string>(s => s.StartsWith("Failed to copy DACPAC to shared DACPAC repositories: "))), Times.Once);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_FailedToEnsureTargetDirectoryExists_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\";
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
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeFalse();
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to ensure that the directory 'C:\\Temp\\Test\\' exists: test directory error"), Times.Once);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_FailedToCopyFile_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\";
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
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeFalse();
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to copy DACPAC to shared DACPAC repository at 'C:\\Temp\\Test\\': test copy error"), Times.Once);
    }

    [Test]
    public async Task Work_ScaffoldingStateModel_NoError_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\;.\\_Deployment\\";
        var targetVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("C:\\projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScaffoldingStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeNull();
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\projectDirectory\\_Deployment\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\projectDirectory\\_Deployment\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
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
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeNull();
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
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\";
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
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeFalse();
        fsaMock.Verify(m => m.EnsureDirectoryExists(It.IsAny<string>()), Times.Never);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.Is<string>(s => s.StartsWith("Failed to copy DACPAC to shared DACPAC repositories: "))), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_FailedToEnsureTargetDirectoryExists_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\";
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
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeFalse();
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to ensure that the directory 'C:\\Temp\\Test\\' exists: test directory error"), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_FailedToCopyFile_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\";
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
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeFalse();
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to copy DACPAC to shared DACPAC repository at 'C:\\Temp\\Test\\': test copy error"), Times.Once);
    }

    [Test]
    public async Task Work_ScriptCreationStateModel_NoError_Async()
    {
        // Arrange
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        configuration.SharedDacpacRepositoryPaths = "C:\\Temp\\Test\\;.\\_Deployment\\";
        var previousVersion = new Version(1, 2, 3);
        Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
        var directories = new DirectoryPaths("C:\\projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged)
        {
            Paths = paths
        };
        var fsaMock = new Mock<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IWorkUnit<ScriptCreationStateModel> unit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock.Object, loggerMock.Object);

        // Act
        await unit.Work(model, CancellationToken.None);

        // Assert
        model.CurrentState.Should().Be(StateModelState.TriedToCopyDacpacToSharedDacpacRepository);
        model.Result.Should().BeNull();
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\Temp\\Test\\"), Times.Once);
        fsaMock.Verify(m => m.EnsureDirectoryExists("C:\\projectDirectory\\_Deployment\\"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\Temp\\Test\\newDacpacPath"), Times.Once);
        fsaMock.Verify(m => m.CopyFile("newDacpacPath", "C:\\projectDirectory\\_Deployment\\newDacpacPath"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Copying DACPAC to shared DACPAC repositories ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
    }
}