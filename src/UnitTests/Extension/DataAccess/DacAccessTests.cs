using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.DataAccess
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;

    [TestFixture]
    public class DacAccessTests
    {
        [Test]
        public void Constructor_ArgumentNullException_XmlFormatService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DacAccess(null));
        }

        [Test]
        public void CreateDeployFilesAsync_ArgumentNullException_PreviousVersionDacpacPath()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();
            IDacAccess da = new DacAccess(xfsMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => da.CreateDeployFilesAsync(null, null, null, false, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CreateDeployFilesAsync_ArgumentNullException_NewVersionDacpacPath()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();
            IDacAccess da = new DacAccess(xfsMock);
            var previousVersionDacpacPath = "path1";

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => da.CreateDeployFilesAsync(previousVersionDacpacPath, null, null, false, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CreateDeployFilesAsync_ArgumentNullException_PublishProfilePath()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();
            IDacAccess da = new DacAccess(xfsMock);
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, null, false, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CreateDeployFilesAsync_InvalidOperationException_CreateNoOutput()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();
            IDacAccess da = new DacAccess(xfsMock);
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, false, false));
        }
    }
}