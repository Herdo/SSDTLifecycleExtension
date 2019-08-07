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
                  .ReturnsAsync(new CreateDeployFilesResult(new[] {"error1", "error2"}));
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
            loggerMock.Verify(m => m.LogErrorAsync("Script creation aborted."), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync("Failed to create script."), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync("error1"), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync("error2"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_PersistScriptError_Async()
        {
            // Arrange
            var testException = new Exception("test exception");
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(new CreateDeployFilesResult("pre script post", "report", "pre ", " post", new PublishProfile()));
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.WriteFileAsync("deployScriptPath", "pre script post"))
                   .ThrowsAsync(testException);
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
            loggerMock.Verify(m => m.LogErrorAsync("Script creation aborted."), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(testException, "Failed to write deploy script"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_PersistReportError_Async()
        {
            // Arrange
            var testException = new Exception("test exception");
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(new CreateDeployFilesResult("pre script post", "report", "pre ", " post", new PublishProfile()));
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.WriteFileAsync("deployReportPath", "report"))
                   .ThrowsAsync(testException);
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
            loggerMock.Verify(m => m.LogErrorAsync("Script creation aborted."), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(testException, "Failed to write deploy report"), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_ScriptDoesNotContainPreDeploymentScript_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(new CreateDeployFilesResult("script post", "report", "pre ", " post", new PublishProfile()));
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
            loggerMock.Verify(m => m.LogErrorAsync("Failed to create complete script. Generated script is missing the pre-deployment script."), Times.Once);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_ScriptDoesNotContainPostDeploymentScript_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(new CreateDeployFilesResult("pre script", "report", "pre ", " post", new PublishProfile()));
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
            loggerMock.Verify(m => m.LogErrorAsync("Failed to create complete script. Generated script is missing the post-deployment script."), Times.Once);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task Work_ScriptCreationStateModel_CreateOnlyScript_Async(bool includePreDeployment, bool includePostDeployment)
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, false))
                  .ReturnsAsync(new CreateDeployFilesResult((includePreDeployment ? "pre " : "") + "script" + (includePostDeployment ? " post" : ""),
                                                            null,
                                                            includePreDeployment ? "pre " : null,
                                                            includePostDeployment ? " post" : null,
                                                            new PublishProfile()));
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
            fsaMock.Verify(m => m.WriteFileAsync("deployScriptPath", (includePreDeployment ? "pre " : "") + "script" + (includePostDeployment ? " post" : "")), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public async Task Work_ScriptCreationStateModel_CreateScriptAndReport_Async(bool includePreDeployment, bool includePostDeployment)
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(new CreateDeployFilesResult((includePreDeployment ? "pre " : "") + "script" + (includePostDeployment ? " post" : ""),
                                                            "report",
                                                            includePreDeployment ? "pre " : null,
                                                            includePostDeployment ? " post" : null,
                                                            new PublishProfile()));
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
            fsaMock.Verify(m => m.WriteFileAsync("deployScriptPath", (includePreDeployment ? "pre " : "") + "script" + (includePostDeployment ? " post" : "")), Times.Once);
            fsaMock.Verify(m => m.WriteFileAsync("deployReportPath", "report"), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        [TestCase(false, false, false, false, false, null)]
        [TestCase(false, false, false, false, true, null)]
        [TestCase(false, false, false, true, false, null)]
        [TestCase(false, false, false, true, true, null)]
        [TestCase(false, false, true, false, false, null)]
        [TestCase(false, false, true, false, true, null)]
        [TestCase(false, false, true, true, false, null)]
        [TestCase(false, false, true, true, true, null)]
        [TestCase(false, true, false, false, false, null)]
        [TestCase(false, true, false, false, true, null)]
        [TestCase(false, true, false, true, false, null)]
        [TestCase(false, true, false, true, true, null)]
        [TestCase(false, true, true, false, false, null)]
        [TestCase(false, true, true, false, true, null)]
        [TestCase(false, true, true, true, false, null)]
        [TestCase(false, true, true, true, true, null)]
        [TestCase(true, false, false, false, false, null)]
        [TestCase(true, false, false, false, true, false)]
        [TestCase(true, false, false, true, false, false)]
        [TestCase(true, false, false, true, true, false)]
        [TestCase(true, false, true, false, false, false)]
        [TestCase(true, false, true, false, true, false)]
        [TestCase(true, false, true, true, false, false)]
        [TestCase(true, false, true, true, true, false)]
        [TestCase(true, true, false, false, false, false)]
        [TestCase(true, true, false, false, true, false)]
        [TestCase(true, true, false, true, false, false)]
        [TestCase(true, true, false, true, true, false)]
        [TestCase(true, true, true, false, false, false)]
        [TestCase(true, true, true, false, true, false)]
        [TestCase(true, true, true, true, false, false)]
        [TestCase(true, true, true, true, true, false)]
        public async Task Work_ScriptCreationStateModel_PublishProfileValidation_Async(bool removeSqlCmdStatements,
                                                                                       bool createNewDatabase,
                                                                                       bool backupDatabaseBeforeChanges,
                                                                                       bool scriptDatabaseOptions,
                                                                                       bool scriptDeployStateChecks,
                                                                                       bool? expectedResult)
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            daMock.Setup(m => m.CreateDeployFilesAsync("previousDacpacPath", "newDacpacPath", "publishProfilePath", true, true))
                  .ReturnsAsync(new CreateDeployFilesResult("pre script post", "report", "pre ", " post", new PublishProfile
                   {
                      CreateNewDatabase = createNewDatabase,
                      BackupDatabaseBeforeChanges = backupDatabaseBeforeChanges,
                      ScriptDatabaseOptions = scriptDatabaseOptions,
                      ScriptDeployStateChecks = scriptDeployStateChecks
                   }));
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CreateDeploymentFilesUnit(daMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.CreateDocumentationWithScriptCreation = true;
            configuration.RemoveSqlCmdStatements = removeSqlCmdStatements;
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
            Assert.AreEqual(expectedResult, model.Result);
            if (removeSqlCmdStatements && createNewDatabase)
            {
                loggerMock.Verify(m => m.LogErrorAsync($"{nameof(PublishProfile.CreateNewDatabase)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true."), Times.Once);
            }
            if (removeSqlCmdStatements && backupDatabaseBeforeChanges)
            {
                loggerMock.Verify(m => m.LogErrorAsync($"{nameof(PublishProfile.BackupDatabaseBeforeChanges)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true."), Times.Once);
            }
            if (removeSqlCmdStatements && scriptDatabaseOptions)
            {
                loggerMock.Verify(m => m.LogErrorAsync($"{nameof(PublishProfile.ScriptDatabaseOptions)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true."), Times.Once);
            }
            if (removeSqlCmdStatements && scriptDeployStateChecks)
            {
                loggerMock.Verify(m => m.LogErrorAsync($"{nameof(PublishProfile.ScriptDeployStateChecks)} cannot bet set to true, when {nameof(ConfigurationModel.RemoveSqlCmdStatements)} is also true."), Times.Once);
            }

            if (expectedResult == null)
            {
                fsaMock.Verify(m => m.WriteFileAsync("deployScriptPath", "pre script post"), Times.Once);
                fsaMock.Verify(m => m.WriteFileAsync("deployReportPath", "report"), Times.Once);
            }
            else
            {
                fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }
    }
}