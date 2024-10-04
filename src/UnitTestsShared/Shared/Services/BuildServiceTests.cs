namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class BuildServiceTests
{
    [Test]
    public async Task BuildProjectAsync_LogException_Async()
    {
        // Arrange
        var testException = new InvalidOperationException("test exception");
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.BuildProjectAsync(project))
               .Throws(testException);
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IBuildService service = new BuildService(vsaMock.Object, fsaMock, loggerMock.Object);

        // Act
        var result = await service.BuildProjectAsync(project);

        // Assert
        result.Should().BeFalse();
        vsaMock.Verify(m => m.BuildProjectAsync(project), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(testException, @"Failed to build C:\TestProject\TestProject.sqlproj"), Times.Once);
    }

    [Test]
    public async Task BuildProjectAsync_Correctly_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var vsaMock = new Mock<IVisualStudioAccess>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = new Mock<ILogger>();
        IBuildService service = new BuildService(vsaMock.Object, fsaMock, loggerMock.Object);

        // Act
        var result = await service.BuildProjectAsync(project);

        // Assert
        result.Should().BeTrue();
        vsaMock.Verify(m => m.BuildProjectAsync(project), Times.Once);
    }

    [Test]
    public void CopyBuildResultAsync_ArgumentException_BinaryDirectoryNotSet()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        const string targetDirectory = @"C:\TestProject\_Deployment\1.0.0";
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var loggerMock = Mock.Of<ILogger>();
        IBuildService service = new BuildService(vsaMock, fsaMock, loggerMock);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.CopyBuildResultAsync(project, targetDirectory));
    }

    [Test]
    public async Task CopyBuildResultAsync_LogError_CannotEnsureDirectoryExists_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.BinaryDirectory = @"C:\TestProject\bin\Output";
        const string targetDirectory = @"C:\TestProject\_Deployment\1.0.0";
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.EnsureDirectoryExists(targetDirectory))
               .Returns("failed to access parent directory");
        var loggerMock = new Mock<ILogger>();
        IBuildService service = new BuildService(vsaMock, fsaMock.Object, loggerMock.Object);

        // Act
        var result = await service.CopyBuildResultAsync(project, targetDirectory);

        // Assert
        result.Should().BeFalse();
        loggerMock.Verify(m => m.LogErrorAsync("Failed to ensure the target directory exists: failed to access parent directory"), Times.Once);
    }

    [Test]
    public async Task CopyBuildResultAsync_LogError_FailedToCopyFiles_Async()
    {
        // Arrange
        var directoryException = new Exception("failed to find files to copy");
        var fileException = new Exception("failed to copy file");
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.BinaryDirectory = @"C:\TestProject\bin\Output";
        const string targetDirectory = @"C:\TestProject\_Deployment\1.0.0";
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.EnsureDirectoryExists(targetDirectory))
               .Returns(null as string);
        fsaMock.Setup(m => m.CopyFiles(project.ProjectProperties.BinaryDirectory, targetDirectory, "*.dacpac"))
               .Returns((new[]
                            {
                                (@"C:\Source\test.dacpac", @"C:\Target\test.dacpac")
                            },
                            new (string File, Exception Exception)[]
                            {
                                (null, directoryException),
                                ("test123.dacpac", fileException)
                            }));
        var loggerMock = new Mock<ILogger>();
        IBuildService service = new BuildService(vsaMock, fsaMock.Object, loggerMock.Object);

        // Act
        var result = await service.CopyBuildResultAsync(project, targetDirectory);

        // Assert
        result.Should().BeFalse();
        loggerMock.Verify(m => m.LogTraceAsync(@"Copied file ""C:\Source\test.dacpac"" to ""C:\Target\test.dacpac"" ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to copy files to the target directory."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(directoryException, "Failed to access the directory"), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(fileException, "Failed to copy file test123.dacpac"), Times.Once);
    }

    [Test]
    public async Task CopyBuildResultAsync_Correctly_Async()
    {
        // Arrange
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        project.ProjectProperties.BinaryDirectory = @"C:\TestProject\bin\Output";
        const string targetDirectory = @"C:\TestProject\_Deployment\1.0.0";
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.EnsureDirectoryExists(targetDirectory))
               .Returns(null as string);
        fsaMock.Setup(m => m.CopyFiles(project.ProjectProperties.BinaryDirectory, targetDirectory, "*.dacpac"))
               .Returns((new[]
                            {
                                (@"C:\Source\test.dacpac", @"C:\Target\test.dacpac")
                            },
                            Array.Empty<(string File, Exception Exception)>()));
        var loggerMock = new Mock<ILogger>();
        IBuildService service = new BuildService(vsaMock, fsaMock.Object, loggerMock.Object);

        // Act
        var result = await service.CopyBuildResultAsync(project, targetDirectory);

        // Assert
        result.Should().BeTrue();
        loggerMock.Verify(m => m.LogTraceAsync(@"Copied file ""C:\Source\test.dacpac"" to ""C:\Target\test.dacpac"" ..."), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
    }
}