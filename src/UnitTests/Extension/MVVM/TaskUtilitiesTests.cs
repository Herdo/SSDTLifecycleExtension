using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.MVVM
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.MVVM;

    [TestFixture]
    public class TaskUtilitiesTests
    {
        [Test]
        public void FireAndForget_ArgumentNullException_Task()
        {
            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => (null as Task).FireAndForget(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void FireAndForget_ArgumentNullException_Command()
        {
            // Arrange
            var task = Task.CompletedTask;

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => task.FireAndForget(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void FireAndForget_ArgumentNullException_Handler()
        {
            // Arrange
            var task = Task.CompletedTask;
            var commandMock = Mock.Of<IAsyncCommand>();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => task.FireAndForget(commandMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void FireAndForget_RunCompletely()
        {
            // Arrange
            var hasRun = false;
            var task = Task.Run(() => hasRun = true);
            var commandMock = Mock.Of<IAsyncCommand>();
            var errorHandlerMock = Mock.Of<IErrorHandler>();

            // Act
            task.FireAndForget(commandMock, errorHandlerMock);

            // Assert
            Assert.That(() => hasRun, Is.True.After(1000, 10), $"{nameof(hasRun)} hasn't been set to true.");
        }

        [Test]
        public void FireAndForget_CallErrorHandlerOnException()
        {
            // Arrange
            var task = Task.Run(() => throw new InvalidOperationException("test exception"));
            var commandMock = Mock.Of<IAsyncCommand>();
            var errorHandlerMock = new Mock<IErrorHandler>();

            // Act
            task.FireAndForget(commandMock, errorHandlerMock.Object);

            // Assert
            errorHandlerMock.Verify(m => m.HandleErrorAsync(commandMock, It.IsNotNull<InvalidOperationException>()), Times.Once);
        }

        [Test]
        public void FireAndForget_CallErrorHandlerOnException_DoNotThrowExceptionFromErrorHandler()
        {
            // Arrange
            var task = Task.Run(() => throw new InvalidOperationException("test exception"));
            var commandMock = Mock.Of<IAsyncCommand>();
            var errorHandlerMock = new Mock<IErrorHandler>();
            errorHandlerMock.Setup(m => m.HandleErrorAsync(commandMock, It.IsNotNull<Exception>()))
                            .ThrowsAsync(new IOException("generic exception while logging"));

            // Act
            Assert.DoesNotThrow(() => task.FireAndForget(commandMock, errorHandlerMock.Object));

            // Assert
            errorHandlerMock.Verify(m => m.HandleErrorAsync(commandMock, It.IsNotNull<InvalidOperationException>()), Times.Once);
        }
    }
}