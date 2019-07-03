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
    public class CopyBuildResultUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_BuildService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CopyBuildResultUnit(null));
        }

        [Test]
        public void Work_ScaffoldingStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var bsMock = new Mock<IBuildService>();
            IWorkUnit<ScaffoldingStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScaffoldingStateModel_CopiedSuccessful_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");
            var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
            {
                Paths = paths
            };
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.NewArtifactsDirectory)).ReturnsAsync(true);
            IWorkUnit<ScaffoldingStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCopyBuildResult, model.CurrentState);
            Assert.IsNull(model.Result);
        }

        [Test]
        public async Task Work_ScaffoldingStateModel_CopyFailed_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");
            var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged)
            {
                Paths = paths
            };
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.NewArtifactsDirectory)).ReturnsAsync(false);
            IWorkUnit<ScaffoldingStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCopyBuildResult, model.CurrentState);
            Assert.IsFalse(model.Result);
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var bsMock = new Mock<IBuildService>();
            IWorkUnit<ScriptCreationStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CopiedSuccessful_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");
            var model = new ScriptCreationStateModel(project, configuration, targetVersion, true, HandleWorkInProgressChanged)
            {
                Paths = paths
            };
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.NewArtifactsDirectory)).ReturnsAsync(true);
            IWorkUnit<ScriptCreationStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCopyBuildResult, model.CurrentState);
            Assert.IsNull(model.Result);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CopyFailed_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");
            var model = new ScriptCreationStateModel(project, configuration, targetVersion, true, HandleWorkInProgressChanged)
            {
                Paths = paths
            };
            var bsMock = new Mock<IBuildService>();
            bsMock.Setup(m => m.CopyBuildResultAsync(project, paths.NewArtifactsDirectory)).ReturnsAsync(false);
            IWorkUnit<ScriptCreationStateModel> unit = new CopyBuildResultUnit(bsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.TriedToCopyBuildResult, model.CurrentState);
            Assert.IsFalse(model.Result);
        }
    }
}