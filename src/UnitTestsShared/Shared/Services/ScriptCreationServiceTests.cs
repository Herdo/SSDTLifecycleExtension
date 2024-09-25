namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class ScriptCreationServiceTests
{
    [Test]
    public async Task CreateAsync_InvalidOperationException_CallCreateWhileRunning_Async()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var invokedSecondTime = false;
        Exception thrownException = null;
        service.IsCreatingChanged += (sender,
                                      args) =>
        {
            if (!service.IsCreating)
                return;
            invokedSecondTime = true;
            thrownException = Assert.Throws<InvalidOperationException>(() => service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None));
        };

        // Act
        await service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None);

        // Assert
        invokedSecondTime.Should().BeTrue();
        thrownException.Should().NotBeNull();
        thrownException.Should().BeOfType<InvalidOperationException>();
    }

    [Test]
    public async Task CreateAsync_CompleteRun_NoWorkUnit_Async()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsCreatingChanged += (sender,
                                      args) =>
        {
            isCreatingList.Add(service.IsCreating);
        };

        // Act
        var result = await service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        isCreatingList.Should().HaveCount(2);
        isCreatingList[0].Should().BeTrue();
        isCreatingList[1].Should().BeFalse();
        vsaMock.Verify(m => m.StartLongRunningTaskIndicatorAsync(), Times.Once);
        vsaMock.Verify(m => m.ClearSSDTLifecycleOutputAsync(), Times.Once);
        vsaMock.Verify(m => m.StopLongRunningTaskIndicatorAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAsync_CompleteRun_ExceptionInStopLongRunningTask_Async()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.StopLongRunningTaskIndicatorAsync())
               .ThrowsAsync(new Exception("test exception"));
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsCreatingChanged += (sender,
                                      args) =>
        {
            isCreatingList.Add(service.IsCreating);
        };

        // Act
        var result = await service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        isCreatingList.Should().HaveCount(2);
        isCreatingList[0].Should().BeTrue();
        isCreatingList[1].Should().BeFalse();
        vsaMock.Verify(m => m.StartLongRunningTaskIndicatorAsync(), Times.Once);
        vsaMock.Verify(m => m.ClearSSDTLifecycleOutputAsync(), Times.Once);
        vsaMock.Verify(m => m.StopLongRunningTaskIndicatorAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAsync_CompleteRun_WithCorrectWorkUnit_Async()
    {
        // Arrange
        var workUnitProvided = false;
        var wuMock = new Mock<IWorkUnit<ScriptCreationStateModel>>();
        var wufMock = new Mock<IWorkUnitFactory>();
        wufMock.Setup(m => m.GetNextWorkUnit(It.IsNotNull<ScriptCreationStateModel>()))
               .Returns<ScriptCreationStateModel>(sm =>
               {
                   if (workUnitProvided)
                       return null;
                   workUnitProvided = true;
                   return wuMock.Object;
               });
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock.Object, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsCreatingChanged += (sender,
                                      args) =>
        {
            isCreatingList.Add(service.IsCreating);
        };

        // Act
        var result = await service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        isCreatingList.Should().HaveCount(2);
        isCreatingList[0].Should().BeTrue();
        isCreatingList[1].Should().BeFalse();
        vsaMock.Verify(m => m.StartLongRunningTaskIndicatorAsync(), Times.Once);
        vsaMock.Verify(m => m.ClearSSDTLifecycleOutputAsync(), Times.Once);
        vsaMock.Verify(m => m.StopLongRunningTaskIndicatorAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAsync_CompleteRun_WithFailedWorkUnit_Async()
    {
        // Arrange
        var workUnitProvided = false;
        var wuMock = new Mock<IWorkUnit<ScriptCreationStateModel>>();
        var wufMock = new Mock<IWorkUnitFactory>();
        wufMock.Setup(m => m.GetNextWorkUnit(It.IsNotNull<ScriptCreationStateModel>()))
               .Returns<ScriptCreationStateModel>(sm =>
               {
                   if (workUnitProvided)
                       return null;
                   workUnitProvided = true;
                   sm.Result = false;
                   return wuMock.Object;
               });
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock.Object, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsCreatingChanged += (sender,
                                      args) =>
        {
            isCreatingList.Add(service.IsCreating);
        };

        // Act
        var result = await service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        isCreatingList.Should().HaveCount(2);
        isCreatingList[0].Should().BeTrue();
        isCreatingList[1].Should().BeFalse();
        vsaMock.Verify(m => m.StartLongRunningTaskIndicatorAsync(), Times.Once);
        vsaMock.Verify(m => m.ClearSSDTLifecycleOutputAsync(), Times.Once);
        vsaMock.Verify(m => m.StopLongRunningTaskIndicatorAsync(), Times.Once);
    }

    [Test]
    public async Task CreateAsync_CompleteRun_WithExceptionInWorkUnit_Async()
    {
        // Arrange
        var workUnitProvided = false;
        var wuMock = new Mock<IWorkUnit<ScriptCreationStateModel>>();
        wuMock.Setup(m => m.Work(It.IsNotNull<ScriptCreationStateModel>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("test exception"));
        var wufMock = new Mock<IWorkUnitFactory>();
        wufMock.Setup(m => m.GetNextWorkUnit(It.IsNotNull<ScriptCreationStateModel>()))
               .Returns<ScriptCreationStateModel>(sm =>
               {
                   if (workUnitProvided)
                       return null;
                   workUnitProvided = true;
                   sm.Result = false;
                   return wuMock.Object;
               });
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock.Object, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var previousVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsCreatingChanged += (sender,
                                      args) =>
        {
            isCreatingList.Add(service.IsCreating);
        };

        // Act
        var result = await service.CreateAsync(project, configuration, previousVersion, false, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        isCreatingList.Should().HaveCount(2);
        isCreatingList[0].Should().BeTrue();
        isCreatingList[1].Should().BeFalse();
        vsaMock.Verify(m => m.StartLongRunningTaskIndicatorAsync(), Times.Once);
        vsaMock.Verify(m => m.ClearSSDTLifecycleOutputAsync(), Times.Once);
        vsaMock.Verify(m => m.StopLongRunningTaskIndicatorAsync(), Times.Once);
    }

    [Test]
    public void IsCreating_DefaultFalse()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock, loggerMock);

        // Act
        var isCreating = service.IsCreating;

        // Assert
        isCreating.Should().BeFalse();
    }
}
