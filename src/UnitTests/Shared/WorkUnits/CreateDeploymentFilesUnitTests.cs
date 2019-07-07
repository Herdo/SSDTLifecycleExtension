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
    public class CreateDeploymentFilesUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_DacAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CreateDeploymentFilesUnit(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CreateDeploymentFilesUnit(daMock, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CreateDeploymentFilesUnit(daMock, fsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_DacAccessError_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync((null, null, new[] {"error1", "error2"}));
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.CreateDocumentationWithScriptCreation = true;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
            var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
            var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
            var paths = new PathCollection(directories, sourcePaths, targetPaths);
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCreateDeploymentFiles, model.CurrentState);
            Assert.IsFalse(model.Result);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("ERROR"))), Times.Exactly(2));
            loggerMock.Verify(m => m.LogAsync("error1"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("error2"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_PersistScriptError_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(("script", "report", null));
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.WriteFileAsync("deployScriptPath", "script"))
                   .ThrowsAsync(new Exception("test exception"));
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.CreateDocumentationWithScriptCreation = true;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
            var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
            var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
            var paths = new PathCollection(directories, sourcePaths, targetPaths);
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCreateDeploymentFiles, model.CurrentState);
            Assert.IsFalse(model.Result);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("ERROR"))), Times.Exactly(2));
            loggerMock.Verify(m => m.LogAsync("ERROR: Failed to write deploy script: test exception"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_PersistReportError_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(("script", "report", null));
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.WriteFileAsync("deployReportPath", "report"))
                   .ThrowsAsync(new Exception("test exception"));
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.CreateDocumentationWithScriptCreation = true;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
            var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
            var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
            var paths = new PathCollection(directories, sourcePaths, targetPaths);
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCreateDeploymentFiles, model.CurrentState);
            Assert.IsFalse(model.Result);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            loggerMock.Verify(m => m.LogAsync(It.Is<string>(s => s.StartsWith("ERROR"))), Times.Exactly(2));
            loggerMock.Verify(m => m.LogAsync("ERROR: Failed to write deploy report: test exception"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CreateOnlyScript_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, false))
                  .ReturnsAsync(("script", null, null));
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.CreateDocumentationWithScriptCreation = false;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
            var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
            var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
            var paths = new PathCollection(directories, sourcePaths, targetPaths);
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCreateDeploymentFiles, model.CurrentState);
            Assert.IsNull(model.Result);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            fsaMock.Verify(m => m.WriteFileAsync("deployScriptPath", "script"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CreateScriptAndReport_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(("script", "report", null));
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.CreateDocumentationWithScriptCreation = true;
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
            var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
            var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
            var paths = new PathCollection(directories, sourcePaths, targetPaths);
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc)
            {
                Paths = paths
            };

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCreateDeploymentFiles, model.CurrentState);
            Assert.IsNull(model.Result);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            fsaMock.Verify(m => m.WriteFileAsync("deployScriptPath", "script"), Times.Once);
            fsaMock.Verify(m => m.WriteFileAsync("deployReportPath", "report"), Times.Once);
        }
    }
}