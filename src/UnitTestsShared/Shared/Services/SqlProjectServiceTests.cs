namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class SqlProjectServiceTests
{
    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_Error_NoProjectDirectory_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggedMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<string>()))
                  .Callback((string message) => loggedMessages.Add(message))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeFalse();
        loggedMessages.Should().ContainSingle()
            .Which.Should().Be(@"Cannot get project directory for C:\");
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_Error_NoXmlRoot_Async()
    {
        // Arrange
        const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>";
        var project = new SqlProject("a", @"C:\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggedErrorMessages = new List<(Exception Exception, string Message)>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()))
                  .Callback((Exception e, string message) => loggedErrorMessages.Add((e, message)))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeFalse();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Exception.Should().NotBeNull();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Message.Should().StartWith(@"Cannot read contents of ""C:\TestProject.sqlproj""");
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_Error_NoName_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("a", @"C:\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggedErrorMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<string>()))
                  .Callback((string message) => loggedErrorMessages.Add(message))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeFalse();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Should().Be(@"Cannot read name of ""C:\TestProject.sqlproj"". " +
                "Please make sure that the \"Name\" is set correctly, e.g. \"MyDatabaseProject\". " +
                "This value has to be set manually in XML.");
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_Error_NoOutputPath_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
    <Name>TestProject</Name>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("a", @"C:\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggedErrorMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<string>()))
                  .Callback((string message) => loggedErrorMessages.Add(message))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeFalse();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Should().Be(@"Cannot read output path of ""C:\TestProject.sqlproj"". " +
                "Please make sure that the \"OutputPath\" for the current configuration is set correctly, e.g. \"bin\\Output\\\". " +
                "This value can be set from your database project => \"Properties\" => \"Build\" => \"Output path\".");
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_Error_NoDacVersion_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
    <Name>TestProject</Name>
    <OutputPath>bin\Output</OutputPath>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("a", @"C:\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggedErrorMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<string>()))
                  .Callback((string message) => loggedErrorMessages.Add(message))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeFalse();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Should().Be(@"Cannot read DacVersion of ""C:\TestProject.sqlproj"". " +
                "Please make sure that the \"DacVersion\" is set correctly, e.g. \"1.0.0\". " +
                "This value can bet set from your database project => \"Properties\" => \"Project Settings\" => \"Output types\" => \"Data-tier Application\" => \"Properties...\" => \"Version\".");
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_UseNameAsSqlTargetName_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
    <Name>TestProject</Name>
    <OutputPath>bin\Output</OutputPath>
    <DacVersion>2.2.3</DacVersion>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeTrue();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        project.ProjectProperties.SqlTargetName.Should().Be("TestProject");
        project.ProjectProperties.BinaryDirectory.Should().Be(@"C:\TestProject\bin\Output");
        project.ProjectProperties.DacVersion.Should().Be(new Version(2, 2, 3));
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_UseSqlTargetNameInsteadOfName_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
    <Name>TestProject</Name>
    <OutputPath>bin\Output</OutputPath>
    <DacVersion>2.2.3</DacVersion>
    <SqlTargetName>TestProjectName</SqlTargetName>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeTrue();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        project.ProjectProperties.SqlTargetName.Should().Be("TestProjectName");
        project.ProjectProperties.BinaryDirectory.Should().Be(@"C:\TestProject\bin\Output");
        project.ProjectProperties.DacVersion.Should().Be(new Version(2, 2, 3));
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_UseNameIfSqlTargetNameIsEmpty_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
    <Name>TestProject</Name>
    <OutputPath>bin\Output</OutputPath>
    <DacVersion>2.2.3</DacVersion>
    <SqlTargetName></SqlTargetName>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeTrue();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        project.ProjectProperties.SqlTargetName.Should().Be("TestProject");
        project.ProjectProperties.BinaryDirectory.Should().Be(@"C:\TestProject\bin\Output");
        project.ProjectProperties.DacVersion.Should().Be(new Version(2, 2, 3));
    }

    [Test]
    public async Task TryLoadSqlProjectPropertiesAsync_IssueWarningIfNameNodeIsDifferentFromProjectName_Async()
    {
        // Arrange
        const string xml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
  <PropertyGroup>
    <Name>TestProject</Name>
    <OutputPath>bin\Output</OutputPath>
    <DacVersion>2.2.3</DacVersion>
  </PropertyGroup>
</Project>";
        var project = new SqlProject("awesomeproject", @"C:\TestProject\TestProject.sqlproj", "c");
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.ReadFileAsync(project.FullName))
               .ReturnsAsync(xml);
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock.Object, loggerMock.Object);

        // Act
        var loadedSuccessfully = await service.TryLoadSqlProjectPropertiesAsync(project);

        // Assert
        loadedSuccessfully.Should().BeTrue();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        project.ProjectProperties.SqlTargetName.Should().Be("TestProject");
        project.ProjectProperties.BinaryDirectory.Should().Be(@"C:\TestProject\bin\Output");
        project.ProjectProperties.DacVersion.Should().Be(new Version(2, 2, 3));
        loggerMock.Verify(m => m.LogWarningAsync("XML node 'Name' doesn't match the actual project name. This could cause an unexpected behavior."), Times.Once);
        loggerMock.Verify(m => m.LogDebugAsync("Value of 'Name' node: TestProject"), Times.Once);
        loggerMock.Verify(m => m.LogDebugAsync("Actual project name: awesomeproject"), Times.Once);
    }

    [Test]
    public async Task TryLoadPathsForScaffoldingAsync_Error_NoProjectDirectory_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\", "c");
        project.ProjectProperties.DacVersion = new Version(2, 0, 0);
        var configuration = new ConfigurationModel();
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggedErrorMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<string>()))
                  .Callback((string message) => loggedErrorMessages.Add(message))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScaffoldingAsync(project, configuration);

        // Assert
        paths.Should().BeNull();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Should().Be(@"Cannot get project directory for C:\");
    }

    [Test]
    [TestCase("TestProfile.publish.xml")]
    [TestCase(ConfigurationModel.UseSinglePublishProfileSpecialKeyword)]
    public async Task TryLoadPathsForScaffoldingAsync_SuccessfullyCreated_Async(string publishProfileConfiguration)
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.SqlTargetName = "TestSqlTarget";
        project.ProjectProperties.DacVersion = new Version(40, 23, 4, 4948);
        var configuration = new ConfigurationModel
        {
            ArtifactsPath = "_TestDeployment",
            PublishProfilePath = publishProfileConfiguration
        };
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(It.IsNotNull<Version>(), configuration))
              .Returns((Version version,
                        ConfigurationModel c) => version.ToString());
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetFilesIn(@"C:\TestProject", "*.publish.xml"))
               .Returns(new[] {"TestProfile.publish.xml"});
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScaffoldingAsync(project, configuration);

        // Assert
        paths.Should().NotBeNull();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        paths.Directories.ProjectDirectory.Should().Be(@"C:\TestProject");
        paths.DeploySources.PublishProfilePath.Should().Be(@"C:\TestProject\TestProfile.publish.xml");
        paths.Directories.LatestArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.Directories.NewArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948");
        paths.DeploySources.NewDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac");
        paths.DeploySources.PreviousDacpacPath.Should().BeNull();
        paths.DeployTargets.DeployScriptPath.Should().BeNull();
        paths.DeployTargets.DeployReportPath.Should().BeNull();
    }

    [Test]
    public async Task TryLoadPathsForScaffoldingAsync_SuccessfullyCreated_NoPublishProfile_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.SqlTargetName = "TestSqlTarget";
        project.ProjectProperties.DacVersion = new Version(40, 23, 4, 4948);
        var configuration = new ConfigurationModel
        {
            ArtifactsPath = "_TestDeployment",
            PublishProfilePath = ConfigurationModel.UseSinglePublishProfileSpecialKeyword
        };
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(It.IsNotNull<Version>(), configuration))
              .Returns((Version version,
                        ConfigurationModel c) => version.ToString());
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetFilesIn(@"C:\TestProject", "*.publish.xml"))
               .Returns(Array.Empty<string>());
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScaffoldingAsync(project, configuration);

        // Assert
        paths.Should().NotBeNull();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        paths.Directories.ProjectDirectory.Should().Be(@"C:\TestProject");
        paths.DeploySources.PublishProfilePath.Should().BeNull();
        paths.Directories.LatestArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.Directories.NewArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948");
        paths.DeploySources.NewDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac");
        paths.DeploySources.PreviousDacpacPath.Should().BeNull();
        paths.DeployTargets.DeployScriptPath.Should().BeNull();
        paths.DeployTargets.DeployReportPath.Should().BeNull();
    }

    [Test]
    public async Task TryLoadPathsForScriptCreationAsync_Error_NoProjectDirectory_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\", "c");
        project.ProjectProperties.DacVersion = new Version(2, 0, 0);
        var configuration = new ConfigurationModel();
        var vsMock = Mock.Of<IVersionService>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggedErrorMessages = new List<string>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<string>()))
                  .Callback((string message) => loggedErrorMessages.Add(message))
                  .Returns(Task.CompletedTask);
        ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(1, 0), false);

        // Assert
        paths.Should().BeNull();
        loggedErrorMessages.Should().ContainSingle()
            .Which.Should().Be(@"Cannot get project directory for C:\");
    }

    [Test]
    [TestCase("TestProfile.publish.xml")]
    [TestCase(ConfigurationModel.UseSinglePublishProfileSpecialKeyword)]
    public async Task TryLoadPathsForScriptCreationAsync_SuccessfullyCreated_Latest_Async(string publishProfileConfiguration)
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.SqlTargetName = "TestSqlTarget";
        project.ProjectProperties.DacVersion = new Version(40, 23, 4, 4948);
        var configuration = new ConfigurationModel
        {
            ArtifactsPath = "_TestDeployment",
            PublishProfilePath = publishProfileConfiguration,
            CreateDocumentationWithScriptCreation = true
        };
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(It.IsNotNull<Version>(), configuration))
              .Returns((Version version,
                        ConfigurationModel c) => version.ToString());
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetFilesIn(@"C:\TestProject", "*.publish.xml"))
               .Returns(new[] { "TestProfile.publish.xml" });
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(40, 23, 3, 4932), true);

        // Assert
        paths.Should().NotBeNull();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        paths.Directories.ProjectDirectory.Should().Be(@"C:\TestProject");
        paths.DeploySources.PublishProfilePath.Should().Be(@"C:\TestProject\TestProfile.publish.xml");
        paths.Directories.LatestArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.Directories.NewArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.DeploySources.NewDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget.dacpac");
        paths.DeploySources.PreviousDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac");
        paths.DeployTargets.DeployScriptPath.Should().Be(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.sql");
        paths.DeployTargets.DeployReportPath.Should().Be(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.xml");
    }

    [Test]
    public async Task TryLoadPathsForScriptCreationAsync_SuccessfullyCreated_Latest_NoPublishProfile_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.SqlTargetName = "TestSqlTarget";
        project.ProjectProperties.DacVersion = new Version(40, 23, 4, 4948);
        var configuration = new ConfigurationModel
        {
            ArtifactsPath = "_TestDeployment",
            PublishProfilePath = ConfigurationModel.UseSinglePublishProfileSpecialKeyword,
            CreateDocumentationWithScriptCreation = true
        };
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(It.IsNotNull<Version>(), configuration))
              .Returns((Version version,
                        ConfigurationModel c) => version.ToString());
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetFilesIn(@"C:\TestProject", "*.publish.xml"))
               .Returns(Array.Empty<string>());
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(40, 23, 3, 4932), true);

        // Assert
        paths.Should().NotBeNull();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        paths.Directories.ProjectDirectory.Should().Be(@"C:\TestProject");
        paths.DeploySources.PublishProfilePath.Should().BeNull();
        paths.Directories.LatestArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.Directories.NewArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.DeploySources.NewDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget.dacpac");
        paths.DeploySources.PreviousDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac");
        paths.DeployTargets.DeployScriptPath.Should().Be(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.sql");
        paths.DeployTargets.DeployReportPath.Should().Be(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.xml");
    }

    [Test]
    [TestCase("TestProfile.publish.xml")]
    [TestCase(ConfigurationModel.UseSinglePublishProfileSpecialKeyword)]
    public async Task TryLoadPathsForScriptCreationAsync_SuccessfullyCreated_SpecificVersion_Async(string publishProfileConfiguration)
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.SqlTargetName = "TestSqlTarget";
        project.ProjectProperties.DacVersion = new Version(40, 23, 4, 4948);
        var configuration = new ConfigurationModel
        {
            ArtifactsPath = "_TestDeployment",
            PublishProfilePath = publishProfileConfiguration,
            CreateDocumentationWithScriptCreation = true
        };
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(It.IsNotNull<Version>(), configuration))
              .Returns((Version version,
                        ConfigurationModel c) => version.ToString());
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetFilesIn(@"C:\TestProject", "*.publish.xml"))
               .Returns(new[] { "TestProfile.publish.xml" });
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(40, 23, 3, 4932), false);

        // Assert
        paths.Should().NotBeNull();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        paths.Directories.ProjectDirectory.Should().Be(@"C:\TestProject");
        paths.DeploySources.PublishProfilePath.Should().Be(@"C:\TestProject\TestProfile.publish.xml");
        paths.Directories.LatestArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.Directories.NewArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948");
        paths.DeploySources.NewDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac");
        paths.DeploySources.PreviousDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac");
        paths.DeployTargets.DeployScriptPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.sql");
        paths.DeployTargets.DeployReportPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.xml");
    }

    [Test]
    public async Task TryLoadPathsForScriptCreationAsync_SuccessfullyCreated_SpecificVersion_NoPublishProfile_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.SqlTargetName = "TestSqlTarget";
        project.ProjectProperties.DacVersion = new Version(40, 23, 4, 4948);
        var configuration = new ConfigurationModel
        {
            ArtifactsPath = "_TestDeployment",
            PublishProfilePath = ConfigurationModel.UseSinglePublishProfileSpecialKeyword,
            CreateDocumentationWithScriptCreation = true
        };
        var vsMock = new Mock<IVersionService>();
        vsMock.Setup(m => m.FormatVersion(It.IsNotNull<Version>(), configuration))
              .Returns((Version version,
                        ConfigurationModel c) => version.ToString());
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetFilesIn(@"C:\TestProject", "*.publish.xml"))
               .Returns(Array.Empty<string>());
        var loggerMock = new Mock<ILogger>();
        ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

        // Act
        var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(40, 23, 3, 4932), false);

        // Assert
        paths.Should().NotBeNull();
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        paths.Directories.ProjectDirectory.Should().Be(@"C:\TestProject");
        paths.DeploySources.PublishProfilePath.Should().BeNull();
        paths.Directories.LatestArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\latest");
        paths.Directories.NewArtifactsDirectory.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948");
        paths.DeploySources.NewDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac");
        paths.DeploySources.PreviousDacpacPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac");
        paths.DeployTargets.DeployScriptPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.sql");
        paths.DeployTargets.DeployReportPath.Should().Be(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.xml");
    }
}