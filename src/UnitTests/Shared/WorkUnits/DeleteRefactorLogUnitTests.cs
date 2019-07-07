using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Enums;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.WorkUnits;

    [TestFixture]
    public class DeleteRefactorLogUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DeleteRefactorLogUnit(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DeleteRefactorLogUnit(fsaMock.Object, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            var vsaMock = new Mock<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DeleteRefactorLogUnit(fsaMock.Object, vsaMock.Object, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new DeleteRefactorLogUnit(fsaMock.Object, vsaMock.Object, loggerMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CompleteRun_NoFilesDeleted_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.TryToCleanDirectory("p", "*.refactorlog"))
                   .Returns(new string[0]);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new DeleteRefactorLogUnit(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.DeleteRefactorlogAfterVersionedScriptGeneration = true;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "l", "b", "c", "d", "e", "f");
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.DeletedRefactorLog, model.CurrentState);
            Assert.IsNull(model.Result);
            fsaMock.Verify(m => m.TryToCleanDirectory("p", "*.refactorlog"), Times.Once);
            vsaMock.Verify(m => m.RemoveItemFromProjectRoot(project, It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogAsync("Deleting refactorlog files ..."), Times.Once);
            loggerMock.Verify(m => m.LogAsync("No files were deleted."), Times.Once);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("Deleted file"))), Times.Never);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CompleteRun_TwoFilesDeleted_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.TryToCleanDirectory("p", "*.refactorlog"))
                   .Returns(new []
                    {
                        "file1.refactorlog",
                        "file2.refactorlog"
                    });
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new DeleteRefactorLogUnit(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.DeleteRefactorlogAfterVersionedScriptGeneration = true;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "l", "b", "c", "d", "e", "f");
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.DeletedRefactorLog, model.CurrentState);
            Assert.IsNull(model.Result);
            fsaMock.Verify(m => m.TryToCleanDirectory("p", "*.refactorlog"), Times.Once);
            vsaMock.Verify(m => m.RemoveItemFromProjectRoot(project, "file1.refactorlog"), Times.Once);
            vsaMock.Verify(m => m.RemoveItemFromProjectRoot(project, "file2.refactorlog"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Deleting refactorlog files ..."), Times.Once);
            loggerMock.Verify(m => m.LogAsync("No files were deleted."), Times.Never);
            loggerMock.Verify(m => m.LogAsync("Deleted file file1.refactorlog ..."), Times.Once);
            loggerMock.Verify(m => m.LogAsync("Deleted file file2.refactorlog ..."), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CompleteRun_NoFilesDeleted_ConfigurationDisabled_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new DeleteRefactorLogUnit(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.DeleteRefactorlogAfterVersionedScriptGeneration = false;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "l", "b", "c", "d", "e", "f");
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.DeletedRefactorLog, model.CurrentState);
            Assert.IsNull(model.Result);
            fsaMock.Verify(m => m.TryToCleanDirectory(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            vsaMock.Verify(m => m.RemoveItemFromProjectRoot(project, It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.IsAny<string>()), Times.Never);
        }
    }
}