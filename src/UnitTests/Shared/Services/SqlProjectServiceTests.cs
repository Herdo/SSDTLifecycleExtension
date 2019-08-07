using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
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

        [Test]
        public void TryLoadSqlProjectPropertiesAsync_ArgumentNullException_Project()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.TryLoadSqlProjectPropertiesAsync(null));
        }

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
            Assert.IsFalse(loadedSuccessfully);
            Assert.AreEqual(1, loggedMessages.Count);
            Assert.AreEqual(@"Cannot get project directory for C:\", loggedMessages[0]);
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
            Assert.IsFalse(loadedSuccessfully);
            Assert.AreEqual(1, loggedErrorMessages.Count);
            Assert.IsNotNull(loggedErrorMessages[0]);
            Assert.IsNotNull(loggedErrorMessages[0].Exception);
            Assert.IsTrue(loggedErrorMessages[0].Message.StartsWith(@"Cannot read contents of C:\TestProject.sqlproj"));
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
            Assert.IsFalse(loadedSuccessfully);
            Assert.AreEqual(1, loggedErrorMessages.Count);
            Assert.AreEqual(@"Cannot read name of C:\TestProject.sqlproj", loggedErrorMessages[0]);
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
            Assert.IsFalse(loadedSuccessfully);
            Assert.AreEqual(1, loggedErrorMessages.Count);
            Assert.AreEqual(@"Cannot read output path of C:\TestProject.sqlproj", loggedErrorMessages[0]);
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
            Assert.IsFalse(loadedSuccessfully);
            Assert.AreEqual(1, loggedErrorMessages.Count);
            Assert.AreEqual(@"Cannot read DacVersion of C:\TestProject.sqlproj", loggedErrorMessages[0]);
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
            Assert.IsTrue(loadedSuccessfully);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual("TestProject", project.ProjectProperties.SqlTargetName);
            Assert.AreEqual(@"C:\TestProject\bin\Output", project.ProjectProperties.BinaryDirectory);
            Assert.AreEqual(new Version(2, 2, 3), project.ProjectProperties.DacVersion);
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
            Assert.IsTrue(loadedSuccessfully);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual("TestProjectName", project.ProjectProperties.SqlTargetName);
            Assert.AreEqual(@"C:\TestProject\bin\Output", project.ProjectProperties.BinaryDirectory);
            Assert.AreEqual(new Version(2, 2, 3), project.ProjectProperties.DacVersion);
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
            Assert.IsTrue(loadedSuccessfully);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual("TestProject", project.ProjectProperties.SqlTargetName);
            Assert.AreEqual(@"C:\TestProject\bin\Output", project.ProjectProperties.BinaryDirectory);
            Assert.AreEqual(new Version(2, 2, 3), project.ProjectProperties.DacVersion);
        }

        [Test]
        public void TryLoadPathsForScaffoldingAsync_ArgumentNullException_Project()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.TryLoadPathsForScaffoldingAsync(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void TryLoadPathsForScaffoldingAsync_ArgumentNullException_Configuration()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.TryLoadPathsForScaffoldingAsync(project, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task TryLoadPathsForScaffoldingAsync_Error_NoProjectDirectory_Async()
        {
            // Arrange
            var project = new SqlProject("a", @"C:\", "c");
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
            Assert.IsNull(paths);
            Assert.AreEqual(1, loggedErrorMessages.Count);
            Assert.AreEqual(@"Cannot get project directory for C:\", loggedErrorMessages[0]);
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
            Assert.IsNotNull(paths);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(@"C:\TestProject", paths.Directories.ProjectDirectory);
            Assert.AreEqual(@"C:\TestProject\TestProfile.publish.xml", paths.DeploySources.PublishProfilePath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.LatestArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948", paths.Directories.NewArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac", paths.DeploySources.NewDacpacPath);
            Assert.IsNull(paths.DeploySources.PreviousDacpacPath);
            Assert.IsNull(paths.DeployTargets.DeployScriptPath);
            Assert.IsNull(paths.DeployTargets.DeployReportPath);
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
                   .Returns(new string[0]);
            var loggerMock = new Mock<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

            // Act
            var paths = await service.TryLoadPathsForScaffoldingAsync(project, configuration);

            // Assert
            Assert.IsNotNull(paths);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(@"C:\TestProject", paths.Directories.ProjectDirectory);
            Assert.AreEqual(string.Empty, paths.DeploySources.PublishProfilePath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.LatestArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948", paths.Directories.NewArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac", paths.DeploySources.NewDacpacPath);
            Assert.IsNull(paths.DeploySources.PreviousDacpacPath);
            Assert.IsNull(paths.DeployTargets.DeployScriptPath);
            Assert.IsNull(paths.DeployTargets.DeployReportPath);
        }

        [Test]
        public void TryLoadPathsForScriptCreationAsync_ArgumentNullException_Project()
        {
            // Arrange
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.TryLoadPathsForScriptCreationAsync(null, null, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void TryLoadPathsForScriptCreationAsync_ArgumentNullException_Configuration()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.TryLoadPathsForScriptCreationAsync(project, null, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void TryLoadPathsForScriptCreationAsync_ArgumentNullException_PreviousVersion()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel();
            var vsMock = Mock.Of<IVersionService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.TryLoadPathsForScriptCreationAsync(project, configuration, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task TryLoadPathsForScriptCreationAsync_Error_NoProjectDirectory_Async()
        {
            // Arrange
            var project = new SqlProject("a", @"C:\", "c");
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
            Assert.IsNull(paths);
            Assert.AreEqual(1, loggedErrorMessages.Count);
            Assert.AreEqual(@"Cannot get project directory for C:\", loggedErrorMessages[0]);
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
            Assert.IsNotNull(paths);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(@"C:\TestProject", paths.Directories.ProjectDirectory);
            Assert.AreEqual(@"C:\TestProject\TestProfile.publish.xml", paths.DeploySources.PublishProfilePath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.LatestArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.NewArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget.dacpac", paths.DeploySources.NewDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac", paths.DeploySources.PreviousDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.sql", paths.DeployTargets.DeployScriptPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.xml", paths.DeployTargets.DeployReportPath);
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
                   .Returns(new string[0]);
            var loggerMock = new Mock<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

            // Act
            var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(40, 23, 3, 4932), true);

            // Assert
            Assert.IsNotNull(paths);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(@"C:\TestProject", paths.Directories.ProjectDirectory);
            Assert.AreEqual(string.Empty, paths.DeploySources.PublishProfilePath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.LatestArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.NewArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget.dacpac", paths.DeploySources.NewDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac", paths.DeploySources.PreviousDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.sql", paths.DeployTargets.DeployScriptPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest\TestSqlTarget_40.23.3.4932_latest.xml", paths.DeployTargets.DeployReportPath);
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
            Assert.IsNotNull(paths);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(@"C:\TestProject", paths.Directories.ProjectDirectory);
            Assert.AreEqual(@"C:\TestProject\TestProfile.publish.xml", paths.DeploySources.PublishProfilePath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.LatestArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948", paths.Directories.NewArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac", paths.DeploySources.NewDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac", paths.DeploySources.PreviousDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.sql", paths.DeployTargets.DeployScriptPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.xml", paths.DeployTargets.DeployReportPath);
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
                   .Returns(new string[0]);
            var loggerMock = new Mock<ILogger>();
            ISqlProjectService service = new SqlProjectService(vsMock.Object, fsaMock.Object, loggerMock.Object);

            // Act
            var paths = await service.TryLoadPathsForScriptCreationAsync(project, configuration, new Version(40, 23, 3, 4932), false);

            // Assert
            Assert.IsNotNull(paths);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            Assert.AreEqual(@"C:\TestProject", paths.Directories.ProjectDirectory);
            Assert.AreEqual(string.Empty, paths.DeploySources.PublishProfilePath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\latest", paths.Directories.LatestArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948", paths.Directories.NewArtifactsDirectory);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget.dacpac", paths.DeploySources.NewDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.3.4932\TestSqlTarget.dacpac", paths.DeploySources.PreviousDacpacPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.sql", paths.DeployTargets.DeployScriptPath);
            Assert.AreEqual(@"C:\TestProject\_TestDeployment\40.23.4.4948\TestSqlTarget_40.23.3.4932_40.23.4.4948.xml", paths.DeployTargets.DeployReportPath);
        }
    }
}