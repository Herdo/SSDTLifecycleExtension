namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits;

[TestFixture]
public class WorkUnitFactoryTests
{
    [Test]
    [TestCase(StateModelState.Undefined)]
    [TestCase(StateModelState.PathsVerified)]
    [TestCase(StateModelState.TriedToCreateDeploymentFiles)]
    [TestCase(StateModelState.ModifiedDeploymentScript)]
    [TestCase(StateModelState.DeletedRefactorLog)]
    [TestCase(StateModelState.DeletedLatestArtifacts)]
    public void GetNextWorkUnit_ScaffoldingStateModel_ArgumentOutOfRangeException_StateModel(StateModelState state)
    {
        // Arrange
        var drMock = new Mock<IDependencyResolver>();
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = state
        };

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentOutOfRangeException>(() => wuf.GetNextWorkUnit(model));
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForInitialized()
    {
        // Arrange
        var spsMock = Mock.Of<ISqlProjectService>();
        var expectedWorkUnit = new LoadSqlProjectPropertiesUnit(spsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<LoadSqlProjectPropertiesUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.Initialized
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<LoadSqlProjectPropertiesUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForProjectPropertiesLoaded()
    {
        // Arrange
        var vsMock = Mock.Of<IVersionService>();
        var expectedWorkUnit = new FormatTargetVersionUnit(vsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<FormatTargetVersionUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.SqlProjectPropertiesLoaded
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<FormatTargetVersionUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForFormattedTargetVersionLoaded()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new ValidateTargetVersionUnit(vsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<ValidateTargetVersionUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.FormattedTargetVersionLoaded
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<ValidateTargetVersionUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForFormattedTargetVersionValidated()
    {
        // Arrange
        var spsMock = Mock.Of<ISqlProjectService>();
        var expectedWorkUnit = new LoadPathsUnit(spsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<LoadPathsUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.FormattedTargetVersionValidated
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<LoadPathsUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForPathsLoaded()
    {
        // Arrange
        var bsMock = Mock.Of<IBuildService>();
        var expectedWorkUnit = new BuildProjectUnit(bsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<BuildProjectUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.PathsLoaded
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<BuildProjectUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForTriedToBuildProject()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new CleanNewArtifactsDirectoryUnit(fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CleanNewArtifactsDirectoryUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToBuildProject
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CleanNewArtifactsDirectoryUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitTriedToCleanArtifactsDirectory()
    {
        // Arrange
        var bsMock = Mock.Of<IBuildService>();
        var expectedWorkUnit = new CopyBuildResultUnit(bsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CopyBuildResultUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCleanArtifactsDirectory
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CopyBuildResultUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForTriedToCopyBuildResult()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CopyDacpacToSharedDacpacRepositoriesUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCopyBuildResult
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CopyDacpacToSharedDacpacRepositoriesUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScaffoldingStateModel_CorrectWorkUnitForTriedToCopyDacpacToSharedDacpacRepository()
    {
        // Arrange
        var drMock = new Mock<IDependencyResolver>();
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeNull();
    }

    [Test]
    [TestCase(StateModelState.Undefined)]
    public void GetNextWorkUnit_ScriptCreationStateModel_ArgumentOutOfRangeException_StateModel(StateModelState state)
    {
        // Arrange
        var drMock = new Mock<IDependencyResolver>();
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = state
        };

        // Act & Assert
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentOutOfRangeException>(() => wuf.GetNextWorkUnit(model));
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForInitialized()
    {
        // Arrange
        var spsMock = Mock.Of<ISqlProjectService>();
        var expectedWorkUnit = new LoadSqlProjectPropertiesUnit(spsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<LoadSqlProjectPropertiesUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.Initialized
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<LoadSqlProjectPropertiesUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForProjectPropertiesLoaded()
    {
        // Arrange
        var vsMock = Mock.Of<IVersionService>();
        var expectedWorkUnit = new FormatTargetVersionUnit(vsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<FormatTargetVersionUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.SqlProjectPropertiesLoaded
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<FormatTargetVersionUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForFormattedTargetVersionLoaded()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new ValidateTargetVersionUnit(vsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<ValidateTargetVersionUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.FormattedTargetVersionLoaded
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<ValidateTargetVersionUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForFormattedTargetVersionValidated()
    {
        // Arrange
        var spsMock = Mock.Of<ISqlProjectService>();
        var expectedWorkUnit = new LoadPathsUnit(spsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<LoadPathsUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.FormattedTargetVersionValidated
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<LoadPathsUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForPathsLoaded()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new VerifyPathsUnit(fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<VerifyPathsUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.PathsLoaded
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<VerifyPathsUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForPathsVerified()
    {
        // Arrange
        var bsMock = Mock.Of<IBuildService>();
        var expectedWorkUnit = new BuildProjectUnit(bsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<BuildProjectUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.PathsVerified
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<BuildProjectUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForTriedToBuildProject()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new CleanNewArtifactsDirectoryUnit(fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CleanNewArtifactsDirectoryUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToBuildProject
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CleanNewArtifactsDirectoryUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitTriedToCleanArtifactsDirectory()
    {
        // Arrange
        var bsMock = Mock.Of<IBuildService>();
        var expectedWorkUnit = new CopyBuildResultUnit(bsMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CopyBuildResultUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCleanArtifactsDirectory
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CopyBuildResultUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForTriedToCopyBuildResult()
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new CopyDacpacToSharedDacpacRepositoriesUnit(fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CopyDacpacToSharedDacpacRepositoriesUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCopyBuildResult
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CopyDacpacToSharedDacpacRepositoriesUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForTriedToCopyDacpacToSharedDacpacRepository()
    {
        // Arrange
        var daMock = Mock.Of<IDacAccess>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new CreateDeploymentFilesUnit(daMock, fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CreateDeploymentFilesUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCopyDacpacToSharedDacpacRepository
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<CreateDeploymentFilesUnit>(), Times.Once);
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForTriedToCreateDeploymentFiles()
    {
        // Arrange
        var mpsMock = Mock.Of<IScriptModifierProviderService>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new ModifyDeploymentScriptUnit(mpsMock, fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<ModifyDeploymentScriptUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.TriedToCreateDeploymentFiles
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeSameAs(expectedWorkUnit);
        drMock.Verify(m => m.Get<ModifyDeploymentScriptUnit>(), Times.Once);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForModifiedDeploymentScript(bool createLatest)
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new DeleteRefactorLogUnit(fsaMock, vsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<DeleteRefactorLogUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, createLatest, HandlerFunc)
        {
            CurrentState = StateModelState.ModifiedDeploymentScript
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        if (createLatest)
        {
            workUnit.Should().BeNull();
            drMock.Verify(m => m.Get<DeleteRefactorLogUnit>(), Times.Never);
        }
        else
        {
            workUnit.Should().BeSameAs(expectedWorkUnit);
            drMock.Verify(m => m.Get<DeleteRefactorLogUnit>(), Times.Once);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForDeletedRefactorLog(bool createLatest)
    {
        // Arrange
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var expectedWorkUnit = new CleanLatestArtifactsDirectoryUnit(fsaMock, loggerMock);
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<CleanLatestArtifactsDirectoryUnit>()).Returns(expectedWorkUnit);
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, createLatest, HandlerFunc)
        {
            CurrentState = StateModelState.DeletedRefactorLog
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        if (createLatest)
        {
            workUnit.Should().BeNull();
            drMock.Verify(m => m.Get<CleanLatestArtifactsDirectoryUnit>(), Times.Never);
        }
        else
        {
            workUnit.Should().BeSameAs(expectedWorkUnit);
            drMock.Verify(m => m.Get<CleanLatestArtifactsDirectoryUnit>(), Times.Once);
        }
    }

    [Test]
    public void GetNextWorkUnit_ScriptCreationStateModel_CorrectWorkUnitForDeletedLatestArtifacts()
    {
        // Arrange
        var drMock = new Mock<IDependencyResolver>();
        IWorkUnitFactory wuf = new WorkUnitFactory(drMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        Task HandlerFunc(bool b) => Task.CompletedTask;
        var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
        {
            CurrentState = StateModelState.DeletedLatestArtifacts
        };

        // Act
        var workUnit = wuf.GetNextWorkUnit(model);

        // Assert
        workUnit.Should().BeNull();
    }
}