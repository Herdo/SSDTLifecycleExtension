using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class AsyncExecutorBaseTests
    {
        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new AsyncExecutorBaseTestImplementation(null, null));
        }

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
            Assert.IsNull(model.Result);
            Assert.AreEqual(2, stateList.Count);
            Assert.IsTrue(stateList[0]);
            Assert.IsFalse(stateList[1]);
            loggerMock.Verify(m => m.LogAsync("started"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Creation was canceled by the user."), Times.Once);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
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
            Assert.IsFalse(model.Result);
            Assert.AreEqual(2, stateList.Count);
            Assert.IsTrue(stateList[0]);
            Assert.IsFalse(stateList[1]);
            loggerMock.Verify(m => m.LogAsync("started"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Creation was canceled by the user."), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
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
            Assert.IsTrue(model.Result);
            Assert.AreEqual(2, stateList.Count);
            Assert.IsTrue(stateList[0]);
            Assert.IsFalse(stateList[1]);
            wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
            loggerMock.Verify(m => m.LogAsync("started"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Creation was canceled by the user."), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
        }

        [Test]
        public async Task DoWorkAsync_ExceptionInWorkUnit_Async()
        {
            // Arrange
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
                  .ThrowsAsync(new Exception("test exception"));

            // Act
            await instance.CallDoWorkAsync(model, cts.Token);

            // Assert
            Assert.IsNull(model.Result);
            Assert.AreEqual(2, stateList.Count);
            Assert.IsTrue(stateList[0]);
            Assert.IsFalse(stateList[1]);
            wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
            loggerMock.Verify(m => m.LogAsync("started"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Creation was canceled by the user."), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Never);
            loggerMock.Verify(m => m.LogAsync("Failed: test exception"), Times.Once);
        }

        [Test]
        public async Task DoWorkAsync_ExceptionInWorkUnitWithExceptionInCatch_Async()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(m => m.LogAsync("Failed: test exception"))
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
                  .ThrowsAsync(new Exception("test exception"));

            // Act
            await instance.CallDoWorkAsync(model, cts.Token);

            // Assert
            Assert.IsNull(model.Result);
            Assert.AreEqual(2, stateList.Count);
            Assert.IsTrue(stateList[0]);
            Assert.IsFalse(stateList[1]);
            wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
            loggerMock.Verify(m => m.LogAsync("started"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Creation was canceled by the user."), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Never);
            loggerMock.Verify(m => m.LogAsync("Failed: test exception"), Times.Once);
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
            Assert.IsTrue(model.Result);
            Assert.AreEqual(2, stateList.Count);
            Assert.IsTrue(stateList[0]);
            Assert.IsFalse(stateList[1]);
            wuMock.Verify(m => m.Work(model, cts.Token), Times.Once);
            loggerMock.Verify(m => m.LogAsync("started"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Creation was canceled by the user."), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("completed stateModel in"))), Times.Once);
        }

        private class AsyncExecutorBaseTestImplementation : AsyncExecutorBase<ScaffoldingStateModel>
        {
            private readonly Func<IWorkUnit<ScaffoldingStateModel>> _workUnitGetter;

            public AsyncExecutorBaseTestImplementation([NotNull] ILogger logger,
                                                       Func<IWorkUnit<ScaffoldingStateModel>> workUnitGetter)
                : base(logger)
            {
                _workUnitGetter = workUnitGetter;
            }

            protected override string GetOperationStartedMessage() => "started";

            protected override string GetOperationCompletedMessage(ScaffoldingStateModel stateModel,
                                                                   long elapsedMilliseconds) =>
                $"completed {nameof(stateModel)} in {elapsedMilliseconds}";

            protected override string GetOperationFailedMessage(Exception exception) => $"Failed: {exception.Message}";

            protected override IWorkUnit<ScaffoldingStateModel> GetNextWorkUnitForStateModel(ScaffoldingStateModel stateModel) => _workUnitGetter();

            internal async Task CallDoWorkAsync(ScaffoldingStateModel stateModel,
                                                CancellationToken cancellationToken) =>
                await DoWorkAsync(stateModel, cancellationToken);
        }
    }
}