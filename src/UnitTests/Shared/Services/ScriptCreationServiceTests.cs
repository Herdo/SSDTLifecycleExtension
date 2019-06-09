using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Factories;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ScriptCreationServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_SqlProjectService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(null, null, null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_BuildService()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(spsMock, null, null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VersionService()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(spsMock, bsMock, null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_ScriptModifierFactory()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var vsMock = Mock.Of<IVersionService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(spsMock, bsMock, vsMock, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var vsMock = Mock.Of<IVersionService>();
            var smfMock = Mock.Of<IScriptModifierFactory>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(spsMock, bsMock, vsMock, smfMock, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var vsMock = Mock.Of<IVersionService>();
            var smfMock = Mock.Of<IScriptModifierFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(spsMock, bsMock, vsMock, smfMock, vsaMock, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var vsMock = Mock.Of<IVersionService>();
            var smfMock = Mock.Of<IScriptModifierFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(spsMock, bsMock, vsMock, smfMock, vsaMock, fsaMock, null));
        }
    }
}