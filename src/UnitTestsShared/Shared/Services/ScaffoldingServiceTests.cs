namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class ScaffoldingServiceTests
{
    [Test]
    public async Task ScaffoldAsync_InvalidOperationException_CallCreateWhileRunning_Async()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock, vsaMock, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var invokedSecondTime = false;
        Exception thrownException = null;
        service.IsScaffoldingChanged += (sender,
                                         args) =>
        {
            if (!service.IsScaffolding)
                return;
            invokedSecondTime = true;
            thrownException = Assert.Throws<InvalidOperationException>(() => service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None));
        };

        // Act
        await service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None);

        // Assert
        invokedSecondTime.Should().BeTrue();
        thrownException.Should().NotBeNull();
        thrownException.Should().BeOfType<InvalidOperationException>();
    }

    [Test]
    public async Task ScaffoldAsync_CompleteRun_NoWorkUnit_Async()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsScaffoldingChanged += (sender,
                                         args) =>
        {
            isCreatingList.Add(service.IsScaffolding);
        };

        // Act
        var result = await service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None);

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
    public async Task ScaffoldAsync_CompleteRun_ExceptionInStopLongRunningTask_Async()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.StopLongRunningTaskIndicatorAsync())
               .ThrowsAsync(new Exception("test exception"));
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsScaffoldingChanged += (sender,
                                         args) =>
        {
            isCreatingList.Add(service.IsScaffolding);
        };

        // Act
        var result = await service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None);

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
    public async Task ScaffoldAsync_CompleteRun_WithCorrectWorkUnit_Async()
    {
        // Arrange
        var workUnitProvided = false;
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        var wufMock = new Mock<IWorkUnitFactory>();
        wufMock.Setup(m => m.GetNextWorkUnit(It.IsNotNull<ScaffoldingStateModel>()))
               .Returns<ScaffoldingStateModel>(sm =>
               {
                   if (workUnitProvided)
                       return null;
                   workUnitProvided = true;
                   return wuMock.Object;
               });
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock.Object, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsScaffoldingChanged += (sender,
                                         args) =>
        {
            isCreatingList.Add(service.IsScaffolding);
        };

        // Act
        var result = await service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None);

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
    public async Task ScaffoldAsync_CompleteRun_WithFailedWorkUnit_Async()
    {
        // Arrange
        var workUnitProvided = false;
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        var wufMock = new Mock<IWorkUnitFactory>();
        wufMock.Setup(m => m.GetNextWorkUnit(It.IsNotNull<ScaffoldingStateModel>()))
               .Returns<ScaffoldingStateModel>(sm =>
               {
                   if (workUnitProvided)
                       return null;
                   workUnitProvided = true;
                   sm.Result = false;
                   return wuMock.Object;
               });
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock.Object, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsScaffoldingChanged += (sender,
                                         args) =>
        {
            isCreatingList.Add(service.IsScaffolding);
        };

        // Act
        var result = await service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None);

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
    public async Task ScaffoldAsync_CompleteRun_WithExceptionInWorkUnit_Async()
    {
        // Arrange
        var workUnitProvided = false;
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        wuMock.Setup(m => m.Work(It.IsNotNull<ScaffoldingStateModel>(), It.IsAny<CancellationToken>()))
              .ThrowsAsync(new Exception("test exception"));
        var wufMock = new Mock<IWorkUnitFactory>();
        wufMock.Setup(m => m.GetNextWorkUnit(It.IsNotNull<ScaffoldingStateModel>()))
               .Returns<ScaffoldingStateModel>(sm =>
               {
                   if (workUnitProvided)
                       return null;
                   workUnitProvided = true;
                   sm.Result = false;
                   return wuMock.Object;
               });
        var vsaMock = new Mock<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock.Object, vsaMock.Object, loggerMock);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var isCreatingList = new List<bool>();
        service.IsScaffoldingChanged += (sender,
                                         args) =>
        {
            isCreatingList.Add(service.IsScaffolding);
        };

        // Act
        var result = await service.ScaffoldAsync(project, configuration, targetVersion, CancellationToken.None);

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
    public void IsScaffolding_DefaultFalse()
    {
        // Arrange
        var wufMock = Mock.Of<IWorkUnitFactory>();
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IScaffoldingService service = new ScaffoldingService(wufMock, vsaMock, loggerMock);

        // Act
        var isCreating = service.IsScaffolding;

        // Assert
        isCreating.Should().BeFalse();
    }
}
