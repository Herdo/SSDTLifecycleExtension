namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class ArtifactsServiceTests
{
    [Test]
    public void GetExistingArtifactVersions_ShowError_CannotDetermineProjectDirectory()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        IArtifactsService service = new ArtifactsService(vsaMock.Object, fsaMock.Object);

        // Act
        var result = service.GetExistingArtifactVersions(project, config);

        // Assert
        result.Should().BeEmpty();
        vsaMock.Verify(m => m.ShowModalError("ERROR: Cannot determine project directory."), Times.Once);
    }

    [Test]
    public void GetExistingArtifactVersions_ShowError_InvalidArtifactsPathConfiguration()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        IArtifactsService service = new ArtifactsService(vsaMock.Object, fsaMock.Object);

        // Act
        var result = service.GetExistingArtifactVersions(project, config);

        // Assert
        result.Should().BeEmpty();
        vsaMock.Verify(m => m.ShowModalError("ERROR: The configured artifacts path is not valid. Please ensure that the configuration is correct."), Times.Once);
    }

    [Test]
    public void GetExistingArtifactVersions_ShowError_ExceptionDuringFileSystemAccess()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
               .Throws(new InvalidOperationException("test exception"));
        IArtifactsService service = new ArtifactsService(vsaMock.Object, fsaMock.Object);

        // Act
        var result = service.GetExistingArtifactVersions(project, config);

        // Assert
        result.Should().BeEmpty();
        vsaMock.Verify(m => m.ShowModalError("ERROR: Failed to open script creation window: test exception"), Times.Once);
    }

    [Test]
    public void GetExistingArtifactVersions_NoDirectories()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
               .Returns(Array.Empty<string>());
        IArtifactsService service = new ArtifactsService(vsaMock.Object, fsaMock.Object);

        // Act
        var result = service.GetExistingArtifactVersions(project, config);

        // Assert
        result.Should().BeEmpty();
        vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetExistingArtifactVersions_NoValidDirectories()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
               .Returns(new []
               {
                   @"C:\TestProject\_Deployment\foo"
               });
        IArtifactsService service = new ArtifactsService(vsaMock.Object, fsaMock.Object);

        // Act
        var result = service.GetExistingArtifactVersions(project, config);

        // Assert
        result.Should().BeEmpty();
        vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void GetExistingArtifactVersions_SuccessfullyParseAndFlagHighestVersion()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.GetDirectoriesIn(@"C:\TestProject\_Deployment"))
               .Returns(new[]
               {
                   @"C:\TestProject\_Deployment\4.0.0",
                   @"C:\TestProject\_Deployment\5.0"
               });
        IArtifactsService service = new ArtifactsService(vsaMock.Object, fsaMock.Object);

        // Act
        var result = service.GetExistingArtifactVersions(project, config);

        // Assert
        result.Should().HaveCount(2);
        result[0].IsNewestVersion.Should().BeTrue();
        result[0].UnderlyingVersion.Should().Be(new Version(5, 0));
        result[1].IsNewestVersion.Should().BeFalse();
        result[1].UnderlyingVersion.Should().Be(new Version(4, 0, 0));
        vsaMock.Verify(m => m.ShowModalError(It.IsAny<string>()), Times.Never);
    }
}