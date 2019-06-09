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
        public void Constructor_ArgumentNullException_VersionService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(null, null, null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_SqlProjectService()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(vsMock, null, null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_BuildService()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var spsMock = Mock.Of<ISqlProjectService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(vsMock, spsMock, null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_ScriptModifierFactory()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(vsMock, spsMock, bsMock, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var smfMock = Mock.Of<IScriptModifierFactory>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(vsMock, spsMock, bsMock, smfMock, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var smfMock = Mock.Of<IScriptModifierFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(vsMock, spsMock, bsMock, smfMock, vsaMock, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var smfMock = Mock.Of<IScriptModifierFactory>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationService(vsMock, spsMock, bsMock, smfMock, vsaMock, fsaMock, null));
        }
    }
}