using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ConfigurationServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(fsaMock, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(fsaMock, vsaMock, null));
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_SqlProject_ArgumentNullException_Project()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.GetConfigurationOrDefaultAsync(null as SqlProject));
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_SqlProject_ArgumentExceptionException_InvalidPathFormat()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);
            var project = new SqlProject("", "", "");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<ArgumentException>(() => service.GetConfigurationOrDefaultAsync(project));
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_SqlProject_InvalidOperationException_InvalidDirectoryPathOfProject()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);
            var project = new SqlProject("", "C:\\", "");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<InvalidOperationException>(() => service.GetConfigurationOrDefaultAsync(project));
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_SqlProject_NoConfigurationFileFound_UseDefault_Async()
        {
            // Arrange
            var exception = new FileNotFoundException("foo");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(false);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync(project);

            // Assert
            Assert.IsNotNull(configuration);
            vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(exception, It.IsAny<string>()), Times.Never);
            Assert.IsTrue(defaultConfiguration.Equals(configuration));
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_SqlProject_ErrorWhileReading_UseDefault_Async()
        {
            // Arrange
            var exception = new FileNotFoundException("foo");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(true);
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Throws(exception);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync(project);

            // Assert
            Assert.IsNotNull(configuration);
            vsaMock.Verify(m => m.ShowModalError(It.Is<string>(s => s.Contains("Accessing the configuration file failed."))), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(exception, It.Is<string>(s => s.Contains("Failed to read the configuration from file"))), Times.Once);
            Assert.IsTrue(defaultConfiguration.Equals(configuration));
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_SqlProject_ConfigurationFound_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(true);
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .ReturnsAsync(() =>
                                     "{  \"ArtifactsPath\": \"__Deployment\",  \"PublishProfilePath\": \"Test.publish.xml\",  " +
                                     "\"SharedDacpacRepositoryPath\": \"C:\\\\Temp\\\\Repository\\\\\", " +
                                     "\"BuildBeforeScriptCreation\": false,  \"CreateDocumentationWithScriptCreation\": true,  " +
                                     "\"CommentOutUnnamedDefaultConstraintDrops\": true, \"RemoveSqlCmdStatements\": true, " +
                                     "\"ReplaceUnnamedDefaultConstraintDrops\": true,  \"VersionPattern\": \"{MAJOR}.0.{BUILD}\",  " +
                                     "\"TrackDacpacVersion\": true,  \"CustomHeader\": \"header\",  \"CustomFooter\": \"footer\"}");
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock, loggerMock);
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync(project);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsFalse(defaultConfiguration.Equals(configuration));
            Assert.AreEqual("__Deployment", configuration.ArtifactsPath);
            Assert.AreEqual("Test.publish.xml", configuration.PublishProfilePath);
            Assert.AreEqual("C:\\Temp\\Repository\\", configuration.SharedDacpacRepositoryPath);
            Assert.IsFalse(configuration.BuildBeforeScriptCreation);
            Assert.IsTrue(configuration.CreateDocumentationWithScriptCreation);
            Assert.IsTrue(configuration.CommentOutUnnamedDefaultConstraintDrops);   // This must be true to cause an validation error for the last assert.
            Assert.IsTrue(configuration.ReplaceUnnamedDefaultConstraintDrops);      // This must be true to cause an validation error for the last assert.
            Assert.AreEqual("{MAJOR}.0.{BUILD}", configuration.VersionPattern);
            Assert.IsTrue(configuration.TrackDacpacVersion);
            Assert.AreEqual("header", configuration.CustomHeader);
            Assert.AreEqual("footer", configuration.CustomFooter);
            Assert.IsTrue(configuration.RemoveSqlCmdStatements);
            Assert.IsTrue(configuration.HasErrors); // This will check if ValidateAll is called correctly.
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_SqlProject_ConfigurationFound_PopulateMissingMemberFromDefaultInstance_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(true);
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .ReturnsAsync(() =>
                                     "{  \"ArtifactsPath\": \"__Deployment\",  " +
                                     "\"BuildBeforeScriptCreation\": false,  \"CreateDocumentationWithScriptCreation\": true,  " +
                                     "\"CommentOutUnnamedDefaultConstraintDrops\": true, \"RemoveSqlCmdStatements\": true, " +
                                     "\"ReplaceUnnamedDefaultConstraintDrops\": true,  " +
                                     "\"TrackDacpacVersion\": true,  \"CustomHeader\": \"header\",  \"CustomFooter\": \"footer\"}");
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock, loggerMock);
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync(project);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsFalse(defaultConfiguration.Equals(configuration));
            Assert.AreEqual("__Deployment", configuration.ArtifactsPath);
            Assert.AreEqual(defaultConfiguration.PublishProfilePath, configuration.PublishProfilePath);
            Assert.AreEqual(defaultConfiguration.SharedDacpacRepositoryPath, configuration.SharedDacpacRepositoryPath);
            Assert.IsFalse(configuration.BuildBeforeScriptCreation);
            Assert.IsTrue(configuration.CreateDocumentationWithScriptCreation);
            Assert.IsTrue(configuration.CommentOutUnnamedDefaultConstraintDrops);   // This must be true to cause an validation error for the last assert.
            Assert.IsTrue(configuration.ReplaceUnnamedDefaultConstraintDrops);      // This must be true to cause an validation error for the last assert.
            Assert.AreEqual(defaultConfiguration.VersionPattern, configuration.VersionPattern);
            Assert.IsTrue(configuration.TrackDacpacVersion);
            Assert.AreEqual("header", configuration.CustomHeader);
            Assert.AreEqual("footer", configuration.CustomFooter);
            Assert.IsTrue(configuration.RemoveSqlCmdStatements);
            Assert.IsTrue(configuration.HasErrors); // This will check if ValidateAll is called correctly.
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_Path_ArgumentNullException_Project()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.GetConfigurationOrDefaultAsync(null as string));
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_Path_ErrorWhileReading_UseDefault_Async()
        {
            // Arrange
            var exception = new FileNotFoundException("foo");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(true);
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Throws(exception);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json");

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsTrue(defaultConfiguration.Equals(configuration));
            vsaMock.Verify(m => m.ShowModalError(It.Is<string>(s => s.Contains("Accessing the configuration file failed."))), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(exception, It.Is<string>(s => s.Contains("Failed to read the configuration from file"))), Times.Once);
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_Path_NoConfigurationFound_UseDefault_Async()
        {
            // Arrange
            var exception = new FileNotFoundException("foo");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(false);
            var vsaMock = new Mock<IVisualStudioAccess>();
            var loggerMock = new Mock<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json");

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsTrue(defaultConfiguration.Equals(configuration));
            vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(exception, It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_Path_ConfigurationFound_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(true);
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .ReturnsAsync(() =>
                                     "{  \"ArtifactsPath\": \"__Deployment\",  \"PublishProfilePath\": \"Test.publish.xml\",  " +
                                     "\"SharedDacpacRepositoryPath\": \"C:\\\\Temp\\\\Repository\\\\\", " +
                                     "\"BuildBeforeScriptCreation\": false,  \"CreateDocumentationWithScriptCreation\": true,  " +
                                     "\"CommentOutUnnamedDefaultConstraintDrops\": true, \"RemoveSqlCmdStatements\": true, " +
                                     "\"ReplaceUnnamedDefaultConstraintDrops\": true,  \"VersionPattern\": \"{MAJOR}.0.{BUILD}\",  " +
                                     "\"TrackDacpacVersion\": true,  \"CustomHeader\": \"header\",  \"CustomFooter\": \"footer\"}");
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock, loggerMock);
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json");

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsFalse(defaultConfiguration.Equals(configuration));
            Assert.AreEqual("__Deployment", configuration.ArtifactsPath);
            Assert.AreEqual("Test.publish.xml", configuration.PublishProfilePath);
            Assert.AreEqual("C:\\Temp\\Repository\\", configuration.SharedDacpacRepositoryPath);
            Assert.IsFalse(configuration.BuildBeforeScriptCreation);
            Assert.IsTrue(configuration.CreateDocumentationWithScriptCreation);
            Assert.IsTrue(configuration.CommentOutUnnamedDefaultConstraintDrops);   // This must be true to cause an validation error for the last assert.
            Assert.IsTrue(configuration.ReplaceUnnamedDefaultConstraintDrops);      // This must be true to cause an validation error for the last assert.
            Assert.AreEqual("{MAJOR}.0.{BUILD}", configuration.VersionPattern);
            Assert.IsTrue(configuration.TrackDacpacVersion);
            Assert.AreEqual("header", configuration.CustomHeader);
            Assert.AreEqual("footer", configuration.CustomFooter);
            Assert.IsTrue(configuration.RemoveSqlCmdStatements);
            Assert.IsTrue(configuration.HasErrors); // This will check if ValidateAll is called correctly.
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_Path_ConfigurationFound_PopulateMissingMemberFromDefaultInstance_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.CheckIfFileExists("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .Returns(true);
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .ReturnsAsync(() =>
                                     "{  \"ArtifactsPath\": \"__Deployment\",  " +
                                     "\"BuildBeforeScriptCreation\": false,  \"CreateDocumentationWithScriptCreation\": true,  " +
                                     "\"CommentOutUnnamedDefaultConstraintDrops\": true, \"RemoveSqlCmdStatements\": true, " +
                                     "\"ReplaceUnnamedDefaultConstraintDrops\": true,  " +
                                     "\"TrackDacpacVersion\": true,  \"CustomHeader\": \"header\",  \"CustomFooter\": \"footer\"}");
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock, loggerMock);
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json");

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsFalse(defaultConfiguration.Equals(configuration));
            Assert.AreEqual("__Deployment", configuration.ArtifactsPath);
            Assert.AreEqual(defaultConfiguration.PublishProfilePath, configuration.PublishProfilePath);
            Assert.AreEqual(defaultConfiguration.SharedDacpacRepositoryPath, configuration.SharedDacpacRepositoryPath);
            Assert.IsFalse(configuration.BuildBeforeScriptCreation);
            Assert.IsTrue(configuration.CreateDocumentationWithScriptCreation);
            Assert.IsTrue(configuration.CommentOutUnnamedDefaultConstraintDrops);   // This must be true to cause an validation error for the last assert.
            Assert.IsTrue(configuration.ReplaceUnnamedDefaultConstraintDrops);      // This must be true to cause an validation error for the last assert.
            Assert.AreEqual(defaultConfiguration.VersionPattern, configuration.VersionPattern);
            Assert.IsTrue(configuration.TrackDacpacVersion);
            Assert.AreEqual("header", configuration.CustomHeader);
            Assert.AreEqual("footer", configuration.CustomFooter);
            Assert.IsTrue(configuration.RemoveSqlCmdStatements);
            Assert.IsTrue(configuration.HasErrors); // This will check if ValidateAll is called correctly.
        }

        [Test]
        public void SaveConfigurationAsync_ArgumentNullException_Project()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.SaveConfigurationAsync(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void SaveConfigurationAsync_ArgumentNullException_Model()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);
            var project = new SqlProject("", "", "");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.SaveConfigurationAsync(project, null));
        }

        [Test]
        public void SaveConfigurationAsync_ArgumentExceptionException_InvalidPathFormat()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);
            var project = new SqlProject("", "", "");
            var model = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<ArgumentException>(() => service.SaveConfigurationAsync(project, model));
        }

        [Test]
        public void SaveConfigurationAsync_InvalidOperationException_InvalidDirectoryPathOfProject()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock, loggerMock);
            var project = new SqlProject("", "C:\\", "");
            var model = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveConfigurationAsync(project, model));
        }

        [Test]
        public async Task SaveConfiguration_FileSystemAccessError_Async()
        {
            // Arrange
            var exception = new IOException("foo");
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.WriteFileAsync(It.IsNotNull<string>(), It.IsNotNull<string>()))
                   .Throws(exception);
            var vsaMock = new Mock<IVisualStudioAccess>();
            SqlProject configurationChangedProject = null;
            var loggerMock = new Mock<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object, loggerMock.Object);
            service.ConfigurationChanged += (sender,
                                             args) => configurationChangedProject = args.Project;
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var model = ConfigurationModel.GetDefault();

            // Act
            var result = await service.SaveConfigurationAsync(project, model);

            // Assert
            Assert.IsFalse(result);
            fsaMock.Verify(m => m.WriteFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json", It.IsNotNull<string>()), Times.Once);
            Assert.IsNull(configurationChangedProject);
            vsaMock.Verify(m => m.AddItemToProjectProperties(It.IsAny<SqlProject>(), It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(exception, "Failed to save the configuration"), Times.Once);
            vsaMock.Verify(m => m.ShowModalError("Failed to save the configuration. Please check the SSDT Lifecycle output window for details."), Times.Once);
        }

        [Test]
        public async Task SaveConfiguration_SavedWithChangeNotification_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.WriteFileAsync(It.IsNotNull<string>(), It.IsNotNull<string>()))
                   .Returns(Task.CompletedTask);
            var vsaMock = new Mock<IVisualStudioAccess>();
            SqlProject configurationChangedProject = null;
            var loggerMock = Mock.Of<ILogger>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object, loggerMock);
            service.ConfigurationChanged += (sender,
                                             args) => configurationChangedProject = args.Project;
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var model = ConfigurationModel.GetDefault();

            // Act
            var result = await service.SaveConfigurationAsync(project, model);

            // Assert
            Assert.IsTrue(result);
            fsaMock.Verify(m => m.WriteFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json", It.IsNotNull<string>()), Times.Once);
            Assert.AreSame(project, configurationChangedProject);
            vsaMock.Verify(m => m.AddItemToProjectProperties(project, "C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"), Times.Once);
        }
    }
}