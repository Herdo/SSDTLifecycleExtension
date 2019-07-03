﻿using NUnit.Framework;

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
    public class CleanLatestArtifactsDirectoryUnitTests
    {
        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CleanLatestArtifactsDirectoryUnit(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new CleanLatestArtifactsDirectoryUnit(fsaMock.Object, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Work_ScriptCreationStateModel_ArgumentNullException_StateModel()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CleanLatestArtifactsDirectoryUnit(fsaMock.Object, loggerMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => unit.Work(null, CancellationToken.None));
        }

        [Test]
        public async Task Work_ScriptCreationStateModel_CompleteRun_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IWorkUnit<ScriptCreationStateModel> unit = new CleanLatestArtifactsDirectoryUnit(fsaMock.Object, loggerMock.Object);
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
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
            Assert.AreEqual(StateModelState.DeletedLatestArtifacts, model.CurrentState);
            Assert.IsNull(model.Result);
            fsaMock.Verify(m => m.TryToCleanDirectory("l"), Times.Once);
            loggerMock.Verify(m => m.LogAsync(It.IsNotNull<string>()), Times.Once);
        }
    }
}