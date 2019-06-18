namespace SSDTLifecycleExtension.UnitTests.Extension.MVVM
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using SSDTLifecycleExtension.MVVM;

    [TestFixture]
    public class AsyncCommandTests
    {
        [Test]
        public void Constructor_ArgumentNullException_Execute()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new AsyncCommand(null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_CanExecute()
        {
            // Arrange
            Task Execute() => Task.CompletedTask;

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new AsyncCommand(Execute, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ErrorHandler()
        {
            // Arrange
            Task Execute() => Task.CompletedTask;
            bool CanExecute() => true;

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new AsyncCommand(Execute, CanExecute, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void RaiseCanExecuteChanged_InvokeEvent()
        {
            // Arrange
            Task Execute() => Task.CompletedTask;
            bool CanExecute() => true;
            var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                                   exception) =>
            {
            });
            object invokedSender = null;
            EventArgs invokedArgs = null;
            var command = new AsyncCommand(Execute, CanExecute, errorHandler);
            command.CanExecuteChanged += (sender,
                                          args) =>
            {
                invokedSender = sender;
                invokedArgs = args;
            };

            // Act
            command.RaiseCanExecuteChanged();

            // Assert
            Assert.IsNotNull(invokedSender);
            Assert.AreSame(command, invokedSender);
            Assert.IsNotNull(invokedArgs);
            Assert.AreSame(EventArgs.Empty, invokedArgs);
        }

        [Test]
        public void CanExecuteWithParam_CallDelegate()
        {
            // Arrange
            Task Execute() => Task.CompletedTask;
            var canExecuteCalled = false;
            bool CanExecute()
            {
                canExecuteCalled = true;
                return true;
            }
            var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                                   exception) =>
            {
            });
            ICommand command = new AsyncCommand(Execute, CanExecute, errorHandler);

            // Act
            var canExecute = command.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
            Assert.IsTrue(canExecuteCalled);
        }

        [Test]
        public void CanExecuteWithoutParam_CallDelegate()
        {
            // Arrange
            Task Execute() => Task.CompletedTask;
            var canExecuteCalled = false;
            bool CanExecute()
            {
                canExecuteCalled = true;
                return true;
            }
            var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                                   exception) =>
            {
            });
            IAsyncCommand command = new AsyncCommand(Execute, CanExecute, errorHandler);

            // Act
            var canExecute = command.CanExecute();

            // Assert
            Assert.IsTrue(canExecute);
            Assert.IsTrue(canExecuteCalled);
        }

        [Test]
        public void ExecuteWithParam_DoNotExecuteWhenCanExecuteReturnsFalse()
        {
            // Arrange
            var executed = false;
            Task Execute()
            {
                executed = true;
                return Task.CompletedTask;
            }
            bool CanExecute() => false;
            var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                                   exception) =>
            {
            });
            ICommand command = new AsyncCommand(Execute, CanExecute, errorHandler);

            // Act
            command.Execute(null);

            // Assert
            Assert.IsFalse(executed);
        }

        [Test]
        public async Task ExecuteAsync_DoNotExecuteWhenCanExecuteReturnsFalse_Async()
        {
            // Arrange
            var executed = false;
            Task Execute()
            {
                executed = true;
                return Task.CompletedTask;
            }
            bool CanExecute() => false;
            var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                                   exception) =>
            {
            });
            IAsyncCommand command = new AsyncCommand(Execute, CanExecute, errorHandler);

            // Act
            await command.ExecuteAsync();

            // Assert
            Assert.IsFalse(executed);
        }

        private class ErrorHandlerTestImplementation : IErrorHandler
        {
            private readonly Action<IAsyncCommand, Exception> _callback;

            public ErrorHandlerTestImplementation([NotNull] Action<IAsyncCommand, Exception> callback)
            {
                _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }

            void IErrorHandler.HandleError(IAsyncCommand command,
                                           Exception exception)
            {
                _callback.Invoke(command, exception);
            }
        }
    }
}