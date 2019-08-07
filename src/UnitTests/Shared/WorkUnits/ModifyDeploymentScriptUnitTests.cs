using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Enums;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.WorkUnits;

    [TestFixture]
    public class ModifyDeploymentScriptUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_ScriptModifierProviderService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ModifyDeploymentScriptUnit(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var mpsMock = Mock.Of<IScriptModifierProviderService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ModifyDeploymentScriptUnit(mpsMock, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var mpsMock = Mock.Of<IScriptModifierProviderService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ModifyDeploymentScriptUnit(mpsMock, fsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var mpsMock = Mock.Of<IScriptModifierProviderService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_NoModifiers_Async()
        {
            // Arrange
            var mpsMock = new Mock<IScriptModifierProviderService>();
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 0);
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, false, HandlerFunc);
            mpsMock.Setup(m => m.GetScriptModifiers(configuration)).Returns(new Dictionary<ScriptModifier, IScriptModifier>());

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.ModifiedDeploymentScript, model.CurrentState);
            Assert.IsNull(model.Result);
            mpsMock.Verify(m => m.GetScriptModifiers(It.IsAny<ConfigurationModel>()), Times.Once);
            fsaMock.Verify(m => m.ReadFileAsync(It.IsAny<string>()), Times.Never);
            fsaMock.Verify(m => m.WriteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CorrectModifierExecution_Async()
        {
            // Arrange
            const string baseScript = "foo bar";
            const string expectedResultScript = "foo bar ab";
            var smHeaderMock = new Mock<IScriptModifier>();
            var smFooterMock = new Mock<IScriptModifier>();
            var mpsMock = new Mock<IScriptModifierProviderService>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFileAsync("deployScriptPath"))
                   .ReturnsAsync(baseScript);
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new ModifyDeploymentScriptUnit(mpsMock.Object, fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 0);
            const bool createLatest = false;
            Task HandlerFunc(bool b) => Task.CompletedTask;
            var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
            var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
            var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
            var paths = new PathCollection(directories, sourcePaths, targetPaths);
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, createLatest, HandlerFunc)
            {
                Paths = paths
            };
            mpsMock.Setup(m => m.GetScriptModifiers(configuration)).Returns(new Dictionary<ScriptModifier, IScriptModifier>
            {
                {ScriptModifier.AddCustomFooter, smFooterMock.Object},
                {ScriptModifier.AddCustomHeader, smHeaderMock.Object}
            });
            smHeaderMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                        .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += " a")
                        .Returns(Task.CompletedTask);
            smFooterMock.Setup(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()))
                        .Callback((ScriptModificationModel modificationModel) => modificationModel.CurrentScript += "b")
                        .Returns(Task.CompletedTask);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.ModifiedDeploymentScript, model.CurrentState);
            Assert.IsNull(model.Result);
            mpsMock.Verify(m => m.GetScriptModifiers(It.IsAny<ConfigurationModel>()), Times.Once);
            fsaMock.Verify(m => m.ReadFileAsync(paths.DeployTargets.DeployScriptPath), Times.Once);
            fsaMock.Verify(m => m.WriteFileAsync(paths.DeployTargets.DeployScriptPath, expectedResultScript), Times.Once);
            loggerMock.Verify(m => m.LogInfoAsync(It.IsAny<string>()), Times.Exactly(2));
            smHeaderMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Once);
            smFooterMock.Verify(m => m.ModifyAsync(It.IsNotNull<ScriptModificationModel>()), Times.Once);
        }
    }
}