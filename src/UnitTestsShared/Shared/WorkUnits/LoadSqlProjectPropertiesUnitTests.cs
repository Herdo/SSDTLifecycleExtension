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
    public class LoadSqlProjectPropertiesUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_SqlProjectService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new LoadSqlProjectPropertiesUnit(null));
        }

        [Test]
        public void Work_ScaffoldingStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var spsMock = new Mock<ISqlProjectService>();
            IWorkUnit<ScaffoldingStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScaffoldingStateModel_LoadedSuccessful_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
            var spsMock = new Mock<ISqlProjectService>();
            spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(true);
            IWorkUnit<ScaffoldingStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.SqlProjectPropertiesLoaded, model.CurrentState);
            Assert.IsNull(model.Result);
        }

        [Test]
        public async Task Work_ScaffoldingStateModel_LoadFailed_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var targetVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScaffoldingStateModel(project, configuration, targetVersion, HandleWorkInProgressChanged);
            var spsMock = new Mock<ISqlProjectService>();
            spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(false);
            IWorkUnit<ScaffoldingStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.SqlProjectPropertiesLoaded, model.CurrentState);
            Assert.IsFalse(model.Result);
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var spsMock = new Mock<ISqlProjectService>();
            IWorkUnit<ScriptCreationStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_LoadedSuccessful_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
            var spsMock = new Mock<ISqlProjectService>();
            spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(true);
            IWorkUnit<ScriptCreationStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.SqlProjectPropertiesLoaded, model.CurrentState);
            Assert.IsNull(model.Result);
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_LoadFailed_Async()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 2, 3);
            Task HandleWorkInProgressChanged(bool arg) => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, HandleWorkInProgressChanged);
            var spsMock = new Mock<ISqlProjectService>();
            spsMock.Setup(m => m.TryLoadSqlProjectPropertiesAsync(project)).ReturnsAsync(false);
            IWorkUnit<ScriptCreationStateModel> unit = new LoadSqlProjectPropertiesUnit(spsMock.Object);

            // Act
            await unit.Work(model, CancellationToken.None);

            // Assert
            Assert.AreEqual(StateModelState.SqlProjectPropertiesLoaded, model.CurrentState);
            Assert.IsFalse(model.Result);
        }
    }
}