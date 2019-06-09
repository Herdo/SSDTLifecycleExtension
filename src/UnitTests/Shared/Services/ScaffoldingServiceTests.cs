using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ScaffoldingServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_SqlProjectService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(null, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_BuildService()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(spsMock, null, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VersionService()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(spsMock, bsMock, null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var vsMock = Mock.Of<IVersionService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(spsMock, bsMock, vsMock, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var spsMock = Mock.Of<ISqlProjectService>();
            var bsMock = Mock.Of<IBuildService>();
            var vsMock = Mock.Of<IVersionService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ScaffoldingService(spsMock, bsMock, vsMock, vsaMock, null));
        }
    }
}