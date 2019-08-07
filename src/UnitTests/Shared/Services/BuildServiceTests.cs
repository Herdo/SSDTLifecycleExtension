using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class BuildServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new BuildService(null, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new BuildService(vsaMock, null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            Assert.Throws<ArgumentNullException>(() => new BuildService(vsaMock, fsaMock, null));
        }

        [Test]
        public void BuildProjectAsync_ArgumentNullException_Project()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IBuildService service = new BuildService(vsaMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.BuildProjectAsync(null));
        }

        [Test]
        public async Task BuildProjectAsync_LogException_Async()
        {
            // Arrange
            var testException = new InvalidOperationException("test exception");
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var vsaMock = new Mock<IVisualStudioAccess>();
            vsaMock.Setup(m => m.BuildProject(project))
                   .Throws(testException);
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = new Mock<ILogger>();
            IBuildService service = new BuildService(vsaMock.Object, fsaMock, loggerMock.Object);

            // Act
            var result = await service.BuildProjectAsync(project);

            // Assert
            Assert.IsFalse(result);
            vsaMock.Verify(m => m.BuildProject(project), Times.Once);
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
            Assert.IsTrue(result);
            vsaMock.Verify(m => m.BuildProject(project), Times.Once);
        }

        [Test]
        public void CopyBuildResultAsync_ArgumentNullException_Project()
        {
            // Arrange
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IBuildService service = new BuildService(vsaMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.CopyBuildResultAsync(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void CopyBuildResultAsync_ArgumentNullException_TargetDirectory()
        {
            // Arrange
            var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IBuildService service = new BuildService(vsaMock, fsaMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.CopyBuildResultAsync(project, null));
            // ReSharper restore AssignNullToNotNullAttribute
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
            Assert.IsFalse(result);
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
                                 @"C:\Target\test.dacpac"
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
            Assert.IsFalse(result);
            loggerMock.Verify(m => m.LogTraceAsync(@"Copied file to C:\Target\test.dacpac ..."), Times.Once);
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
                                 @"C:\Target\test.dacpac"
                             },
                             new (string File, Exception Exception)[0]));
            var loggerMock = new Mock<ILogger>();
            IBuildService service = new BuildService(vsaMock, fsaMock.Object, loggerMock.Object);

            // Act
            var result = await service.CopyBuildResultAsync(project, targetDirectory);

            // Assert
            Assert.IsTrue(result);
            loggerMock.Verify(m => m.LogTraceAsync(@"Copied file to C:\Target\test.dacpac ..."), Times.Once);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
            loggerMock.Verify(m => m.LogErrorAsync(It.IsAny<string>()), Times.Never);
        }
    }
}