using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class VisualStudioLoggerTests
    {
        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new VisualStudioLogger(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_DocumentationBaseUrl()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new VisualStudioLogger(vsaMock.Object, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void DocumentationBaseUrl_GetCorrectValue()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            const string documentationBaseUrl = "foobar";

            // Act
            ILogger logger = new VisualStudioLogger(vsaMock.Object, documentationBaseUrl);

            // Assert
            Assert.AreEqual(documentationBaseUrl, logger.DocumentationBaseUrl);
        }

        [Test]
        public void LogTraceAsync_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogTraceAsync(null));
        }

        [Test]
        public async Task LogTraceAsync_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act
            await logger.LogTraceAsync("test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync("TRACE: test message"), Times.Once);
        }

        [Test]
        public void LogDebugAsync_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogDebugAsync(null));
        }

        [Test]
        public async Task LogDebugAsync_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act
            await logger.LogDebugAsync("test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync("DEBUG: test message"), Times.Once);
        }

        [Test]
        public void LogInfoAsync_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogInfoAsync(null));
        }

        [Test]
        public async Task LogInfoAsync_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act
            await logger.LogInfoAsync("test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync("INFO: test message"), Times.Once);
        }

        [Test]
        public void LogWarningAsync_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogWarningAsync(null));
        }

        [Test]
        public async Task LogWarningAsync_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act
            await logger.LogWarningAsync("test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync("WARNING: test message"), Times.Once);
        }

        [Test]
        public void LogErrorAsync_WithException_ArgumentNullException_Exception()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogErrorAsync(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void LogErrorAsync_WithException_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");
            var exception = new Exception("test");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogErrorAsync(exception, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task LogErrorAsync_WithException_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");
            var exception = new Exception("test exception");

            // Act
            await logger.LogErrorAsync(exception, "test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync(It.Is<string>(s => s.StartsWith("ERROR: test message")
                                                                           && s.Contains(exception.Message)
                                                                           && s.Contains(exception.GetType().FullName))),
                           Times.Once);
        }

        [Test]
        public void LogErrorAsync_WithoutException_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogErrorAsync(null));
        }

        [Test]
        public async Task LogErrorAsync_WithoutException_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act
            await logger.LogErrorAsync("test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync("ERROR: test message"), Times.Once);
        }

        [Test]
        public void LogCriticalAsync_WithException_ArgumentNullException_Exception()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogCriticalAsync(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void LogCriticalAsync_WithException_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");
            var exception = new Exception("test");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogCriticalAsync(exception, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task LogCriticalAsync_WithException_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");
            var exception = new Exception("test exception");

            // Act
            await logger.LogCriticalAsync(exception, "test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync(It.Is<string>(s => s.StartsWith("CRITICAL: test message")
                                                                           && s.Contains(exception.Message)
                                                                           && s.Contains(exception.GetType().FullName))),
                           Times.Once);
        }

        [Test]
        public void LogCriticalAsync_WithoutException_ArgumentNullException_Message()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => logger.LogCriticalAsync(null));
        }

        [Test]
        public async Task LogCriticalAsync_WithoutException_LogCorrectMessageAsync()
        {
            // Arrange
            var vsaMock = new Mock<IVisualStudioAccess>();
            ILogger logger = new VisualStudioLogger(vsaMock.Object, "foo");

            // Act
            await logger.LogCriticalAsync("test message");

            // Assert
            vsaMock.Verify(m => m.LogToOutputPanelAsync("CRITICAL: test message"), Times.Once);
        }
    }
}