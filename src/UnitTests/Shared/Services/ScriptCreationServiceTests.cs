using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Factories;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ScriptCreationServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_WorkUnitFactory()
        {
            // Arrange
            var loggerMock = Mock.Of<ILogger>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
             var e = Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(null, null, loggerMock));
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            Assert.AreEqual("workUnitFactory", e.ParamName);
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var loggerMock = Mock.Of<ILogger>();
            var wufMock = Mock.Of<IWorkUnitFactory>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            var e = Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(wufMock, null, loggerMock));
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            Assert.AreEqual("visualStudioAccess", e.ParamName);
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            var e = Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(wufMock, vsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute

            // Assert
            Assert.AreEqual("logger", e.ParamName);
        }

        [Test]
        public void CreateAsync_ArgumentNullException_Project()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.CreateAsync(null, null, null, false, CancellationToken.None));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CreateAsync_ArgumentNullException_Configuration()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock, loggerMock);
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.CreateAsync(project, null, null, false, CancellationToken.None));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CreateAsync_ArgumentNullException_PreviousVersion()
        {
            // Arrange
            var wufMock = Mock.Of<IWorkUnitFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IScriptCreationService service = new ScriptCreationService(wufMock, vsaMock, loggerMock);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.CreateAsync(project, configuration, null, false, CancellationToken.None));
            // ReSharper restore AssignNullToNotNullAttribute
        }

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
            Assert.IsTrue(invokedSecondTime);
            Assert.IsNotNull(thrownException);
            Assert.IsInstanceOf<InvalidOperationException>(thrownException);
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
            Assert.IsTrue(result);
            Assert.AreEqual(2, isCreatingList.Count);
            Assert.IsTrue(isCreatingList[0]);
            Assert.IsFalse(isCreatingList[1]);
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
            Assert.IsTrue(result);
            Assert.AreEqual(2, isCreatingList.Count);
            Assert.IsTrue(isCreatingList[0]);
            Assert.IsFalse(isCreatingList[1]);
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
            Assert.IsTrue(result);
            Assert.AreEqual(2, isCreatingList.Count);
            Assert.IsTrue(isCreatingList[0]);
            Assert.IsFalse(isCreatingList[1]);
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
            Assert.IsFalse(result);
            Assert.AreEqual(2, isCreatingList.Count);
            Assert.IsTrue(isCreatingList[0]);
            Assert.IsFalse(isCreatingList[1]);
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
            Assert.IsFalse(result);
            Assert.AreEqual(2, isCreatingList.Count);
            Assert.IsTrue(isCreatingList[0]);
            Assert.IsFalse(isCreatingList[1]);
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
            Assert.IsFalse(isCreating);
        }
    }
}