using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.DataAccess
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.SqlServer.Dac;
    using Moq;
    using SSDTLifecycleExtension.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts;
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
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DacAccess(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DacAccess(xfsMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CreateDeployFilesAsync_ArgumentNullException_PreviousVersionDacpacPath()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            IDacAccess da = new DacAccess(xfsMock, fsaMock);

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
            var fsaMock = Mock.Of<IFileSystemAccess>();
            IDacAccess da = new DacAccess(xfsMock, fsaMock);
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
            var fsaMock = Mock.Of<IFileSystemAccess>();
            IDacAccess da = new DacAccess(xfsMock, fsaMock);
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
            var fsaMock = Mock.Of<IFileSystemAccess>();
            IDacAccess da = new DacAccess(xfsMock, fsaMock);
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, false, false));
        }

        [Test]
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedNull_Async()
        {
            // Arrange
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";
            var xfsMock = Mock.Of<IXmlFormatService>();
            var fileOpenException1 = new Exception("Test exception1");
            var fileOpenException2 = new Exception("Test exception2");
            var fileOpenException3 = new Exception("Test exception3");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(previousVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns(new SecureResult<DacPackage>(null, fileOpenException1));
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns(new SecureResult<DacPackage>(null, fileOpenException2));
            fsaMock.Setup(m => m.ReadFromStream(publishProfilePath, It.IsNotNull<Func<Stream, DacDeployOptions>>()))
                   .Returns(new SecureResult<DacDeployOptions>(null, fileOpenException3));
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (deployScriptContent, deployReportContent, errors) = await da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, true, true);

            // Assert
            Assert.IsNull(deployScriptContent);
            Assert.IsNull(deployReportContent);
            Assert.IsNotNull(errors);
            Assert.AreEqual(3, errors.Length);
            Assert.AreEqual("Error reading previous DACPAC: Test exception1", errors[0]);
            Assert.AreEqual("Error reading new DACPAC: Test exception2", errors[1]);
            Assert.AreEqual("Error reading publish profile: Test exception3", errors[2]);
        }

        [Test]
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedEmptyStream_Async()
        {
            // Arrange
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";
            var xfsMock = Mock.Of<IXmlFormatService>();

            Stream CreateEmptyStream()
            {
                return new MemoryStream();
            }

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(previousVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(CreateEmptyStream()), null));
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(CreateEmptyStream()), null));
            fsaMock.Setup(m => m.ReadFromStream(publishProfilePath, It.IsNotNull<Func<Stream, DacDeployOptions>>()))
                   .Returns((string path,
                             Func<Stream, DacDeployOptions> consumer) => new SecureResult<DacDeployOptions>(consumer(CreateEmptyStream()), null));
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (deployScriptContent, deployReportContent, errors) = await da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, true, true);

            // Assert
            Assert.IsNull(deployScriptContent);
            Assert.IsNull(deployReportContent);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("Could not load package.", errors[0]);
        }
    }
}