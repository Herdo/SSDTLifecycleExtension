using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class SqlProjectServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_VersionService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new SqlProjectService(null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new SqlProjectService(vsMock, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new SqlProjectService(vsMock, fsaMock, null));
        }
    }
}