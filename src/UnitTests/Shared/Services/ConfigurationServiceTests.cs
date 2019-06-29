using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
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
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new ConfigurationService(fsaMock, null));
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_ArgumentNullException_Project()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.GetConfigurationOrDefaultAsync(null));
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_ArgumentExceptionException_InvalidPathFormat()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);
            var project = new SqlProject("", "", "");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<ArgumentException>(() => service.GetConfigurationOrDefaultAsync(project));
        }

        [Test]
        public void GetConfigurationOrDefaultAsync_InvalidOperationException_InvalidDirectoryPathOfProject()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);
            var project = new SqlProject("", "C:\\", "");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<InvalidOperationException>(() => service.GetConfigurationOrDefaultAsync(project));
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_NoConfigurationFound_UseDefault_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json")).ReturnsAsync(() => null as string);
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock);
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync(project);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsTrue(defaultConfiguration.Equals(configuration));
        }

        [Test]
        public async Task GetConfigurationOrDefaultAsync_ConfigurationFound_Async()
        {
            // Arrange
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.ReadFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"))
                   .ReturnsAsync(() =>
                                     "{  \"ArtifactsPath\": \"__Deployment\",  \"PublishProfilePath\": \"Test.publish.xml\",  " +
                                     "\"BuildBeforeScriptCreation\": false,  \"CreateDocumentationWithScriptCreation\": true,  " +
                                     "\"CommentOutUnnamedDefaultConstraintDrops\": true,  " +
                                     "\"ReplaceUnnamedDefaultConstraintDrops\": true,  \"VersionPattern\": \"{MAJOR}.0.{BUILD}\",  " +
                                     "\"TrackDacpacVersion\": true,  \"CustomHeader\": \"header\",  \"CustomFooter\": \"footer\"}");
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock);
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var defaultConfiguration = ConfigurationModel.GetDefault();

            // Act
            var configuration = await service.GetConfigurationOrDefaultAsync(project);

            // Assert
            Assert.IsNotNull(configuration);
            Assert.IsFalse(defaultConfiguration.Equals(configuration));
            Assert.AreEqual("__Deployment", configuration.ArtifactsPath);
            Assert.AreEqual("Test.publish.xml", configuration.PublishProfilePath);
            Assert.IsFalse(configuration.BuildBeforeScriptCreation);
            Assert.IsTrue(configuration.CreateDocumentationWithScriptCreation);
            Assert.IsTrue(configuration.CommentOutUnnamedDefaultConstraintDrops);   // This must be true to cause an validation error for the last assert.
            Assert.IsTrue(configuration.ReplaceUnnamedDefaultConstraintDrops);      // This must be true to cause an validation error for the last assert.
            Assert.AreEqual("{MAJOR}.0.{BUILD}", configuration.VersionPattern);
            Assert.IsTrue(configuration.TrackDacpacVersion);
            Assert.AreEqual("header", configuration.CustomHeader);
            Assert.AreEqual("footer", configuration.CustomFooter);
            Assert.IsTrue(configuration.HasErrors); // This will check if ValidateAll is called correctly.
        }

        [Test]
        public void SaveConfigurationAsync_ArgumentNullException_Project()
        {
            // Arrange
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);

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
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);
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
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);
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
            IConfigurationService service = new ConfigurationService(fsaMock, vsaMock);
            var project = new SqlProject("", "C:\\", "");
            var model = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveConfigurationAsync(project, model));
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
            IConfigurationService service = new ConfigurationService(fsaMock.Object, vsaMock.Object);
            service.ConfigurationChanged += (sender,
                                             args) => configurationChangedProject = args.Project;
            var project = new SqlProject("", "C:\\Temp\\Test\\Test.sqlproj", "");
            var model = ConfigurationModel.GetDefault();

            // Act
            await service.SaveConfigurationAsync(project, model);

            // Assert
            fsaMock.Verify(m => m.WriteFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json", It.IsNotNull<string>()), Times.Once);
            Assert.AreSame(project, configurationChangedProject);
            vsaMock.Verify(m => m.AddItemToProjectProperties(project, "C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"), Times.Once);
        }
    }
}