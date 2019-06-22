using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.DataAccess
{
    using System;
    using System.IO;
    using System.Linq;
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
        private static Stream GetEmbeddedResourceStream(string resourceName)
        {
            var testAssembly = typeof(DacAccessTests).Assembly;
            var resourceNames = testAssembly.GetManifestResourceNames();
            var fullResourceName = resourceNames.Single(m => m.EndsWith(resourceName));
            return testAssembly.GetManifestResourceStream(fullResourceName);
        }

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
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedEmptyStream_PreviousDacpac_Async()
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

        [Test]
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedEmptyStream_NewDacpac_Async()
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
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(GetEmbeddedResourceStream("TestDatabase_Empty.dacpac")), null));
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(CreateEmptyStream()), null));
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

        [Test]
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedEmptyStream_PublishProfile_Async()
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
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(GetEmbeddedResourceStream("TestDatabase_Empty.dacpac")), null));
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(GetEmbeddedResourceStream("TestDatabase_WithAuthorTable.dacpac")), null));
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
            Assert.AreEqual("Could not read profile xml.", errors[0]);
        }

        [Test]
        public async Task CreateDeployFilesAsync_CorrectCreation_Async()
        {
            // Arrange
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";
            var xfsMock = new Mock<IXmlFormatService>();
            xfsMock.Setup(m => m.FormatDeployReport(It.IsNotNull<string>())).Returns((string s) => s);

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(previousVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(GetEmbeddedResourceStream("TestDatabase_Empty.dacpac")), null));
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) => new SecureResult<DacPackage>(consumer(GetEmbeddedResourceStream("TestDatabase_WithAuthorTable.dacpac")), null));
            fsaMock.Setup(m => m.ReadFromStream(publishProfilePath, It.IsNotNull<Func<Stream, DacDeployOptions>>()))
                   .Returns((string path,
                             Func<Stream, DacDeployOptions> consumer) => new SecureResult<DacDeployOptions>(consumer(GetEmbeddedResourceStream("TestDatabase.publish.xml")), null));
            IDacAccess da = new DacAccess(xfsMock.Object, fsaMock.Object);

            // Act
            var (deployScriptContent, deployReportContent, errors) = await da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, true, true);

            // Assert
            Assert.IsNotNull(deployScriptContent);
            Assert.IsNotNull(deployReportContent);
            Assert.IsNull(errors);
            xfsMock.Verify(m => m.FormatDeployReport(It.IsNotNull<string>()), Times.Once);
            // Verify script
            var productionIndex = deployScriptContent.IndexOf("PRODUCTION", StringComparison.InvariantCulture);
            Assert.IsTrue(productionIndex > 0);
            var onErrorIndex = deployScriptContent.IndexOf(":on error exit", StringComparison.InvariantCulture);
            Assert.IsTrue(onErrorIndex > productionIndex);
            var changeDatabaseIndex = deployScriptContent.IndexOf("USE [$(DatabaseName)]", StringComparison.InvariantCulture);
            Assert.IsTrue(changeDatabaseIndex > onErrorIndex);
            var createAuthorPrintIndex = deployScriptContent.IndexOf("[dbo].[Author]...';", StringComparison.InvariantCulture);
            Assert.IsTrue(createAuthorPrintIndex > changeDatabaseIndex);
            var createAuthorTableIndex = deployScriptContent.IndexOf("CREATE TABLE [dbo].[Author]", StringComparison.InvariantCulture);
            Assert.IsTrue(createAuthorTableIndex > createAuthorPrintIndex);
            // Verify report
            Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-8""?><DeploymentReport xmlns=""http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02""><Alerts /><Operations><Operation Name=""Create""><Item Value=""[dbo].[Author]"" Type=""SqlTable"" /></Operation></Operations></DeploymentReport>",
                            deployReportContent);
        }
    }
}