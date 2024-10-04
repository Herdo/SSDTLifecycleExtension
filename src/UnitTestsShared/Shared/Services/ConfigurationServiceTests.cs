namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class ConfigurationServiceTests
{
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
        configuration.Should().Be(defaultConfiguration);
        vsaMock.Verify(m => m.ShowModalErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(exception, It.IsAny<string>()), Times.Never);
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
        configuration.Should().Be(defaultConfiguration);
        vsaMock.Verify(m => m.ShowModalErrorAsync(It.Is<string>(s => s.Contains("Accessing the configuration file failed."))), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(exception, It.Is<string>(s => s.Contains("Failed to read the configuration from file"))), Times.Once);
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
        configuration.Should().NotBe(defaultConfiguration);
        configuration.ArtifactsPath.Should().Be("__Deployment");
        configuration.PublishProfilePath.Should().Be("Test.publish.xml");
        configuration.SharedDacpacRepositoryPaths.Should().Be("C:\\Temp\\Repository\\");
        configuration.BuildBeforeScriptCreation.Should().BeFalse();
        configuration.CreateDocumentationWithScriptCreation.Should().BeTrue();
        configuration.CommentOutUnnamedDefaultConstraintDrops.Should().BeTrue();   // This must be true to cause an validation error for the last assert.
        configuration.ReplaceUnnamedDefaultConstraintDrops.Should().BeTrue();      // This must be true to cause an validation error for the last assert.
        configuration.VersionPattern.Should().Be("{MAJOR}.0.{BUILD}");
        configuration.TrackDacpacVersion.Should().BeTrue();
        configuration.CustomHeader.Should().Be("header");
        configuration.CustomFooter.Should().Be("footer");
        configuration.RemoveSqlCmdStatements.Should().BeTrue();
        configuration.HasErrors.Should().BeTrue(); // This will check if ValidateAll is called correctly.
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
        configuration.Should().NotBe(defaultConfiguration);
        configuration.ArtifactsPath.Should().Be("__Deployment");
        configuration.PublishProfilePath.Should().Be(defaultConfiguration.PublishProfilePath);
        configuration.SharedDacpacRepositoryPaths.Should().Be(defaultConfiguration.SharedDacpacRepositoryPaths);
        configuration.BuildBeforeScriptCreation.Should().BeFalse();
        configuration.CreateDocumentationWithScriptCreation.Should().BeTrue();
        configuration.CommentOutUnnamedDefaultConstraintDrops.Should().BeTrue();   // This must be true to cause an validation error for the last assert.
        configuration.ReplaceUnnamedDefaultConstraintDrops.Should().BeTrue();      // This must be true to cause an validation error for the last assert.
        configuration.VersionPattern.Should().Be(defaultConfiguration.VersionPattern);
        configuration.TrackDacpacVersion.Should().BeTrue();
        configuration.CustomHeader.Should().Be("header");
        configuration.CustomFooter.Should().Be("footer");
        configuration.RemoveSqlCmdStatements.Should().BeTrue();
        configuration.HasErrors.Should().BeTrue(); // This will check if ValidateAll is called correctly.
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
        configuration.Should().Be(defaultConfiguration);
        vsaMock.Verify(m => m.ShowModalErrorAsync(It.Is<string>(s => s.Contains("Accessing the configuration file failed."))), Times.Once);
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
        configuration.Should().Be(defaultConfiguration);
        vsaMock.Verify(m => m.ShowModalErrorAsync(It.IsAny<string>()), Times.Never);
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
        configuration.Should().NotBe(defaultConfiguration);
        configuration.ArtifactsPath.Should().Be("__Deployment");
        configuration.PublishProfilePath.Should().Be("Test.publish.xml");
        configuration.SharedDacpacRepositoryPaths.Should().Be("C:\\Temp\\Repository\\");
        configuration.BuildBeforeScriptCreation.Should().BeFalse();
        configuration.CreateDocumentationWithScriptCreation.Should().BeTrue();
        configuration.CommentOutUnnamedDefaultConstraintDrops.Should().BeTrue();   // This must be true to cause an validation error for the last assert.
        configuration.ReplaceUnnamedDefaultConstraintDrops.Should().BeTrue();      // This must be true to cause an validation error for the last assert.
        configuration.VersionPattern.Should().Be("{MAJOR}.0.{BUILD}");
        configuration.TrackDacpacVersion.Should().BeTrue();
        configuration.CustomHeader.Should().Be("header");
        configuration.CustomFooter.Should().Be("footer");
        configuration.RemoveSqlCmdStatements.Should().BeTrue();
        configuration.HasErrors.Should().BeTrue(); // This will check if ValidateAll is called correctly.
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
        configuration.Should().NotBe(defaultConfiguration);
        configuration.ArtifactsPath.Should().Be("__Deployment");
        configuration.PublishProfilePath.Should().Be(defaultConfiguration.PublishProfilePath);
        configuration.SharedDacpacRepositoryPaths.Should().Be(defaultConfiguration.SharedDacpacRepositoryPaths);
        configuration.BuildBeforeScriptCreation.Should().BeFalse();
        configuration.CreateDocumentationWithScriptCreation.Should().BeTrue();
        configuration.CommentOutUnnamedDefaultConstraintDrops.Should().BeTrue();   // This must be true to cause an validation error for the last assert.
        configuration.ReplaceUnnamedDefaultConstraintDrops.Should().BeTrue();      // This must be true to cause an validation error for the last assert.
        configuration.VersionPattern.Should().Be(defaultConfiguration.VersionPattern);
        configuration.TrackDacpacVersion.Should().BeTrue();
        configuration.CustomHeader.Should().Be("header");
        configuration.CustomFooter.Should().Be("footer");
        configuration.RemoveSqlCmdStatements.Should().BeTrue();
        configuration.HasErrors.Should().BeTrue(); // This will check if ValidateAll is called correctly.
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
        result.Should().BeFalse();
        fsaMock.Verify(m => m.WriteFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json", It.IsNotNull<string>()), Times.Once);
        configurationChangedProject.Should().BeNull();
        vsaMock.Verify(m => m.AddConfigFileToProjectPropertiesAsync(It.IsAny<SqlProject>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(exception, "Failed to save the configuration"), Times.Once);
        vsaMock.Verify(m => m.ShowModalErrorAsync("Failed to save the configuration. Please check the SSDT Lifecycle output window for details."), Times.Once);
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
        result.Should().BeTrue();
        fsaMock.Verify(m => m.WriteFileAsync("C:\\Temp\\Test\\Properties\\ssdtlifecycle.json", It.IsNotNull<string>()), Times.Once);
        configurationChangedProject.Should().BeSameAs(project);
        vsaMock.Verify(m => m.AddConfigFileToProjectPropertiesAsync(project, "C:\\Temp\\Test\\Properties\\ssdtlifecycle.json"), Times.Once);
    }
}