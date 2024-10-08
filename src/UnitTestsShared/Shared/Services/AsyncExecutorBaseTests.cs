﻿namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class AsyncExecutorBaseTests
{
    [Test]
    public async Task DoWorkAsync_CancellationRequested_Async()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var instance = new AsyncExecutorBaseTestImplementation(loggerMock.Object, () =>
        {
            Assert.Fail("This shouldn't be called");
            return null;
        });
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var stateList = new List<bool>();
        Task HandlerFunc(bool b)
        {
            stateList.Add(b);
            return Task.CompletedTask;
        }
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        await instance.CallDoWorkAsync(model, cts.Token);

        // Assert
        model.Result.Should().BeNull();
        stateList.Should().HaveCount(2);
        stateList[0].Should().BeTrue();
        stateList[1].Should().BeFalse();
        loggerMock.Verify(m => m.LogInfoAsync("started"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Creation was canceled by the user."), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_PreviousUnitFailed_Async()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var instance = new AsyncExecutorBaseTestImplementation(loggerMock.Object, () =>
        {
            Assert.Fail("This shouldn't be called");
            return null;
        });
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var stateList = new List<bool>();
        Task HandlerFunc(bool b)
        {
            stateList.Add(b);
            return Task.CompletedTask;
        }
        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc)
        {
            Result = false
        };

        // Act
        await instance.CallDoWorkAsync(model, CancellationToken.None);

        // Assert
        model.Result.Should().BeFalse();
        stateList.Should().HaveCount(2);
        stateList[0].Should().BeTrue();
        stateList[1].Should().BeFalse();
        loggerMock.Verify(m => m.LogInfoAsync("started"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Creation was canceled by the user."), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_CompleteRun_Async()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var invokedOnce = false;
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        var instance = new AsyncExecutorBaseTestImplementation(loggerMock.Object, () =>
        {
            if (invokedOnce)
                return null;
            invokedOnce = true;
            return wuMock.Object;
        });
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var stateList = new List<bool>();
        Task HandlerFunc(bool b)
        {
            stateList.Add(b);
            return Task.CompletedTask;
        }

        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc);
        var cts = new CancellationTokenSource();

        // Act
        await instance.CallDoWorkAsync(model, cts.Token);

        // Assert
        model.Result.Should().BeTrue();
        stateList.Should().HaveCount(2);
        stateList[0].Should().BeTrue();
        stateList[1].Should().BeFalse();
        wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("started"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Creation was canceled by the user."), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_ExceptionInWorkUnit_Async()
    {
        // Arrange
        var testException = new Exception("test exception");
        var loggerMock = new Mock<ILogger>();
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        var instance = new AsyncExecutorBaseTestImplementation(loggerMock.Object, () => wuMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var stateList = new List<bool>();
        Task HandlerFunc(bool b)
        {
            stateList.Add(b);
            return Task.CompletedTask;
        }

        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc);
        var cts = new CancellationTokenSource();
        wuMock.Setup(m => m.Work(model, cts.Token))
              .ThrowsAsync(testException);

        // Act
        await instance.CallDoWorkAsync(model, cts.Token);

        // Assert
        model.Result.Should().BeNull();
        stateList.Should().HaveCount(2);
        stateList[0].Should().BeTrue();
        stateList[1].Should().BeFalse();
        wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("started"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Creation was canceled by the user."), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(testException, "operation failed."), Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_ExceptionInWorkUnitWithExceptionInCatch_Async()
    {
        // Arrange
        var testException = new Exception("test exception");
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(testException, "operation failed."))
                  .ThrowsAsync(new Exception("catch exception"));
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        var instance = new AsyncExecutorBaseTestImplementation(loggerMock.Object, () => wuMock.Object);
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var stateList = new List<bool>();
        Task HandlerFunc(bool b)
        {
            stateList.Add(b);
            return Task.CompletedTask;
        }

        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc);
        var cts = new CancellationTokenSource();
        wuMock.Setup(m => m.Work(model, cts.Token))
              .ThrowsAsync(testException);

        // Act
        await instance.CallDoWorkAsync(model, cts.Token);

        // Assert
        model.Result.Should().BeNull();
        stateList.Should().HaveCount(2);
        stateList[0].Should().BeTrue();
        stateList[1].Should().BeFalse();
        wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("started"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Creation was canceled by the user."), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(testException, "operation failed."), Times.Once);
    }

    [Test]
    public async Task DoWorkAsync_ExceptionInFinally_Async()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var invokedOnce = false;
        var wuMock = new Mock<IWorkUnit<ScaffoldingStateModel>>();
        var instance = new AsyncExecutorBaseTestImplementation(loggerMock.Object, () =>
        {
            if (invokedOnce)
                return null;
            invokedOnce = true;
            return wuMock.Object;
        });
        var project = new SqlProject("a", "b", "c");
        var configuration = ConfigurationModel.GetDefault();
        var targetVersion = new Version(1, 0);
        var stateList = new List<bool>();
        Task HandlerFunc(bool b)
        {
            stateList.Add(b);
            if (stateList.Count == 2)
                throw new Exception("test exception");
            return Task.CompletedTask;
        }

        var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandlerFunc);
        var cts = new CancellationTokenSource();

        // Act
        await instance.CallDoWorkAsync(model, cts.Token);

        // Assert
        model.Result.Should().BeTrue();
        stateList.Should().HaveCount(2);
        stateList[0].Should().BeTrue();
        stateList[1].Should().BeFalse();
        wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("started"), Times.Once);
        loggerMock.Verify(m => m.LogInfoAsync("Creation was canceled by the user."), Times.Never);
        loggerMock.Verify(m => m.LogInfoAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
    }

    private class AsyncExecutorBaseTestImplementation : AsyncExecutorBase<ScaffoldingStateModel>
    {
        private readonly Func<IWorkUnit<ScaffoldingStateModel>> _workUnitGetter;

        public AsyncExecutorBaseTestImplementation(ILogger logger,
                Func<IWorkUnit<ScaffoldingStateModel>> workUnitGetter)
            : base(logger)
        {
            _workUnitGetter = workUnitGetter;
        }

        protected override string GetOperationStartedMessage() => "started";

        protected override string GetOperationCompletedMessage(ScaffoldingStateModel stateModel,
                                                               long elapsedMilliseconds) =>
            $"completed {nameof(stateModel)} in {elapsedMilliseconds}";

        protected override string GetOperationFailedMessage() => "operation failed.";

        protected override IWorkUnit<ScaffoldingStateModel> GetNextWorkUnitForStateModel(ScaffoldingStateModel stateModel) => _workUnitGetter();

        internal async Task CallDoWorkAsync(ScaffoldingStateModel stateModel,
                                            CancellationToken cancellationToken) =>
            await DoWorkAsync(stateModel, cancellationToken);
    }
}