using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.DataAccess
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.SqlServer.Dac;
    using Microsoft.SqlServer.Dac.Model;
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
                   .Returns(new SecureStreamResult<DacPackage>(null, null, fileOpenException1));
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns(new SecureStreamResult<DacPackage>(null, null, fileOpenException2));
            fsaMock.Setup(m => m.ReadFromStream(publishProfilePath, It.IsNotNull<Func<Stream, DacDeployOptions>>()))
                   .Returns(new SecureStreamResult<DacDeployOptions>(null, null, fileOpenException3));
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
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedCorruptStream_PreviousDacpac_Async()
        {
            // Arrange
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";
            var xfsMock = Mock.Of<IXmlFormatService>();

            Stream CreateEmptyStream()
            {
                return new MemoryStream(new byte[]
                {
                    1,
                    2,
                    234,
                    14
                });
            }

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(previousVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = CreateEmptyStream();
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (deployScriptContent, deployReportContent, errors) = await da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, true, true);

            // Assert
            Assert.IsNull(deployScriptContent);
            Assert.IsNull(deployReportContent);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsNotEmpty(errors[0]);
        }

        [Test]
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedCorruptStream_NewDacpac_Async()
        {
            // Arrange
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";
            var xfsMock = Mock.Of<IXmlFormatService>();

            Stream CreateCorruptStream()
            {
                return new MemoryStream(new byte[]
                {
                    1,
                    2,
                    234,
                    14
                });
            }

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(previousVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_Empty.dacpac");
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = CreateCorruptStream();
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (deployScriptContent, deployReportContent, errors) = await da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, true, true);

            // Assert
            Assert.IsNull(deployScriptContent);
            Assert.IsNull(deployReportContent);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsNotEmpty(errors[0]);
        }

        [Test]
        public async Task CreateDeployFilesAsync_Errors_ReadFromStreamReturnedCorruptStream_PublishProfile_Async()
        {
            // Arrange
            var previousVersionDacpacPath = "path1";
            var newVersionDacpacPath = "path2";
            var publishProfilePath = "path3";
            var xfsMock = Mock.Of<IXmlFormatService>();

            Stream CreateCorruptStream()
            {
                return new MemoryStream(new byte[]
                {
                    1,
                    2,
                    234,
                    14
                });
            }

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(previousVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_Empty.dacpac");
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_WithAuthorTable.dacpac");
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            fsaMock.Setup(m => m.ReadFromStream(publishProfilePath, It.IsNotNull<Func<Stream, DacDeployOptions>>()))
                   .Returns((string path,
                             Func<Stream, DacDeployOptions> consumer) =>
                    {
                        var stream = CreateCorruptStream();
                        return new SecureStreamResult<DacDeployOptions>(stream, consumer(stream), null);
                    });
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (deployScriptContent, deployReportContent, errors) = await da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, true, true);

            // Assert
            Assert.IsNull(deployScriptContent);
            Assert.IsNull(deployReportContent);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsNotEmpty(errors[0]);
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
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_Empty.dacpac");
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            fsaMock.Setup(m => m.ReadFromStream(newVersionDacpacPath, It.IsNotNull<Func<Stream, DacPackage>>()))
                   .Returns((string path,
                             Func<Stream, DacPackage> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_WithAuthorTable.dacpac");
                        return new SecureStreamResult<DacPackage>(stream, consumer(stream), null);
                    });
            fsaMock.Setup(m => m.ReadFromStream(publishProfilePath, It.IsNotNull<Func<Stream, DacDeployOptions>>()))
                   .Returns((string path,
                             Func<Stream, DacDeployOptions> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase.publish.xml");
                        return new SecureStreamResult<DacDeployOptions>(stream, consumer(stream), null);
                    });
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

        [Test]
        public void GetDefaultConstraintsAsync_ArgumentNullException_DacpacPath()
        {
            // Arrange
            var xfsMock = Mock.Of<IXmlFormatService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            IDacAccess da = new DacAccess(xfsMock, fsaMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => da.GetDefaultConstraintsAsync(null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task GetDefaultConstraintsAsync_Errors_ReadFromStreamReturnedCorruptStream_Async()
        {
            // Arrange
            const string dacpacPath = "pathToDacpac";
            var xfsMock = Mock.Of<IXmlFormatService>();

            Stream CreateCorruptStream()
            {
                return new MemoryStream(new byte[]
                {
                    1,
                    2,
                    234,
                    14
                });
            }

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(dacpacPath, It.IsNotNull<Func<Stream, TSqlModel>>()))
                   .Returns((string path,
                             Func<Stream, TSqlModel> consumer) =>
                   {
                       var stream = CreateCorruptStream();
                       return new SecureStreamResult<TSqlModel>(stream, consumer(stream), null);
                   });
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (defaultConstraints, errors) = await da.GetDefaultConstraintsAsync(dacpacPath);

            // Assert
            Assert.IsNull(defaultConstraints);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.IsNotEmpty(errors[0]);
        }

        [Test]
        public async Task GetDefaultConstraintsAsync_Errors_ReadFromStreamReturnedNull_Async()
        {
            // Arrange
            const string dacpacPath = "pathToDacpac";
            var xfsMock = Mock.Of<IXmlFormatService>();

            var fileOpenException = new Exception("Test exception");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(dacpacPath, It.IsNotNull<Func<Stream, TSqlModel>>()))
                   .Returns(new SecureStreamResult<TSqlModel>(null, null, fileOpenException));
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (defaultConstraints, errors) = await da.GetDefaultConstraintsAsync(dacpacPath);

            // Assert
            Assert.IsNull(defaultConstraints);
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Length);
            Assert.AreEqual("Error reading DACPAC: Test exception", errors[0]);
        }

        [Test]
        public async Task GetDefaultConstraintsAsync_CorrectCreation_MoreConstraints_Async()
        {
            // Arrange
            const string dacpacPath = "pathToDacpac";
            var xfsMock = Mock.Of<IXmlFormatService>();

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(dacpacPath, It.IsNotNull<Func<Stream, TSqlModel>>()))
                   .Returns((string path,
                             Func<Stream, TSqlModel> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_AuthorWithDefaultConstraints.dacpac");
                        return new SecureStreamResult<TSqlModel>(stream, consumer(stream), null);
                    });
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (defaultConstraints, errors) = await da.GetDefaultConstraintsAsync(dacpacPath);

            // Assert
            Assert.IsNotNull(defaultConstraints);
            Assert.IsNull(errors);
            Assert.AreEqual(3, defaultConstraints.Length);
            var orderedConstraints = defaultConstraints.OrderBy(m => m.ColumnName)
                                                       .ToArray();
            // First constraint
            Assert.AreEqual("dbo", orderedConstraints[0].TableSchema);
            Assert.AreEqual("Author", orderedConstraints[0].TableName);
            Assert.AreEqual("Birthday", orderedConstraints[0].ColumnName);
            Assert.AreEqual("DF_Birthday_Today", orderedConstraints[0].ConstraintName);
            // Second constraint
            Assert.AreEqual("dbo", orderedConstraints[1].TableSchema);
            Assert.AreEqual("Author", orderedConstraints[1].TableName);
            Assert.AreEqual("FirstName", orderedConstraints[1].ColumnName);
            Assert.AreEqual("DF_FirstName_Empty", orderedConstraints[1].ConstraintName);
            // Third constraint
            Assert.AreEqual("dbo", orderedConstraints[2].TableSchema);
            Assert.AreEqual("Author", orderedConstraints[2].TableName);
            Assert.AreEqual("LastName", orderedConstraints[2].ColumnName);
            Assert.AreEqual(null, orderedConstraints[2].ConstraintName);
        }

        [Test]
        public async Task GetDefaultConstraintsAsync_CorrectCreation_SingleConstraints_Async()
        {
            // Arrange
            const string dacpacPath = "pathToDacpac";
            var xfsMock = Mock.Of<IXmlFormatService>();

            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFromStream(dacpacPath, It.IsNotNull<Func<Stream, TSqlModel>>()))
                   .Returns((string path,
                             Func<Stream, TSqlModel> consumer) =>
                    {
                        var stream = GetEmbeddedResourceStream("TestDatabase_AuthorWithLessDefaultConstraints.dacpac");
                        return new SecureStreamResult<TSqlModel>(stream, consumer(stream), null);
                    });
            IDacAccess da = new DacAccess(xfsMock, fsaMock.Object);

            // Act
            var (defaultConstraints, errors) = await da.GetDefaultConstraintsAsync(dacpacPath);

            // Assert
            Assert.IsNotNull(defaultConstraints);
            Assert.IsNull(errors);
            Assert.AreEqual(1, defaultConstraints.Length);
            Assert.AreEqual("dbo", defaultConstraints[0].TableSchema);
            Assert.AreEqual("Author", defaultConstraints[0].TableName);
            Assert.AreEqual("Birthday", defaultConstraints[0].ColumnName);
            Assert.AreEqual("DF_Birthday_Today", defaultConstraints[0].ConstraintName);
        }
    }
}