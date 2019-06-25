using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.WorkUnits
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.Enums;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.WorkUnits;

    [TestFixture]
    public class BuildProjectUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_BuildService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new BuildProjectUnit(null));
        }

        [Test]
        public void Work_ScaffoldingStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var bsMock = new Mock<IBuildService>();
            IWorkUnit<ScaffoldingStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScaffoldingStateModel_BuiltSuccessful_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(true);
            IWorkUnit<ScaffoldingStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToBuildProject, model.CurrentState);
            Assert.IsNull(model.Result);
        }

        [Test]
        public async Task Work_ScaffoldingStateModel_BuildFailed_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(false);
            IWorkUnit<ScaffoldingStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToBuildProject, model.CurrentState);
            Assert.IsFalse(model.Result);
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var bsMock = new Mock<IBuildService>();
            IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_SkipBuild_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.BuildBeforeScriptCreation = false;
            var previousVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(true);
            IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToBuildProject, model.CurrentState);
            Assert.IsNull(model.Result);
            bsMock.Verify(m => m.BuildProjectAsync(project), Times.Never);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_BuiltSuccessful_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            configuration.BuildBeforeScriptCreation = true;
            var previousVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(true);
            IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToBuildProject, model.CurrentState);
            Assert.IsNull(model.Result);
            bsMock.Verify(m => m.BuildProjectAsync(project), Times.Once);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_BuildFailed_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.BuildProjectAsync(project)).ReturnsAsync(false);
            IWorkUnit<ScriptCreationStateModel> unit = new BuildProjectUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToBuildProject, model.CurrentState);
            Assert.IsFalse(model.Result);
        }
    }
}