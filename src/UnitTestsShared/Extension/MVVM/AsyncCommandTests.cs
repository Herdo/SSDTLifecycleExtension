namespace SSDTLifecycleExtension.UnitTests.Extension.MVVM;

[TestFixture]
public class AsyncCommandTests
{
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
        invokedSender.Should().BeSameAs(command);
        invokedArgs.Should().BeSameAs(EventArgs.Empty);
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
        canExecute.Should().BeTrue();
        canExecuteCalled.Should().BeTrue();
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
        canExecute.Should().BeTrue();
        canExecuteCalled.Should().BeTrue();
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
        executed.Should().BeFalse();
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
        executed.Should().BeFalse();
    }

    [Test]
    public void ExecuteWithParam_CorrectExecutionWithCanExecuteChanged()
    {
        // Arrange
        var executed = false;
        Task Execute()
        {
            executed = true;
            return Task.CompletedTask;
        }
        bool CanExecute() => true;
        var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                               exception) =>
        {
        });
        IAsyncCommand command = new AsyncCommand(Execute, CanExecute, errorHandler);
        var invokedSenderList = new List<object>();
        var invokedArgsList = new List<EventArgs>();
        var invokedCanExecuteStateList = new List<bool>();
        command.CanExecuteChanged += (sender,
                                      args) =>
        {
            if (sender != null)
                invokedSenderList.Add(sender);
            if (args != null)
                invokedArgsList.Add(args);
            invokedCanExecuteStateList.Add(command.CanExecute());
        };

        // Act
        command.Execute(null);

        // Assert
        executed.Should().BeTrue();
        invokedSenderList.Should().HaveCount(2);
        invokedSenderList[0].Should().BeSameAs(command);
        invokedSenderList[1].Should().BeSameAs(command);
        invokedArgsList.Should().HaveCount(2);
        invokedArgsList[0].Should().BeSameAs(EventArgs.Empty);
        invokedArgsList[1].Should().BeSameAs(EventArgs.Empty);
        invokedCanExecuteStateList.Should().HaveCount(2);
        invokedCanExecuteStateList[0].Should().BeFalse(); // Cannot execute during first execution, even when the CanExecute delegate returns true.
        invokedCanExecuteStateList[1].Should().BeTrue(); // Can execute after the execution has finished.
    }

    [Test]
    public async Task ExecuteAsync_CorrectExecutionWithCanExecuteChanged_Async()
    {
        // Arrange
        var executed = false;
        Task Execute()
        {
            executed = true;
            return Task.CompletedTask;
        }
        bool CanExecute() => true;
        var errorHandler = new ErrorHandlerTestImplementation((cmd,
                                                               exception) =>
        {
        });
        IAsyncCommand command = new AsyncCommand(Execute, CanExecute, errorHandler);
        var invokedSenderList = new List<object>();
        var invokedArgsList = new List<EventArgs>();
        var invokedCanExecuteStateList = new List<bool>();
        command.CanExecuteChanged += (sender,
                                      args) =>
        {
            if (sender != null)
                invokedSenderList.Add(sender);
            if (args != null)
                invokedArgsList.Add(args);
            invokedCanExecuteStateList.Add(command.CanExecute());
        };

        // Act
        await command.ExecuteAsync();

        // Assert
        executed.Should().BeTrue();
        invokedSenderList.Should().HaveCount(2);
        invokedSenderList[0].Should().BeSameAs(command);
        invokedSenderList[1].Should().BeSameAs(command);
        invokedArgsList.Should().HaveCount(2);
        invokedArgsList[0].Should().BeSameAs(EventArgs.Empty);
        invokedArgsList[1].Should().BeSameAs(EventArgs.Empty);
        invokedCanExecuteStateList.Should().HaveCount(2);
        invokedCanExecuteStateList[0].Should().BeFalse(); // Cannot execute during first execution, even when the CanExecute delegate returns true.
        invokedCanExecuteStateList[1].Should().BeTrue(); // Can execute after the execution has finished.
    }

    private class ErrorHandlerTestImplementation : IErrorHandler
    {
        private readonly Action<IAsyncCommand, Exception> _callback;

        public ErrorHandlerTestImplementation(Action<IAsyncCommand, Exception> callback)
        {
            _callback = callback;
        }

        Task IErrorHandler.HandleErrorAsync(IAsyncCommand command,
                                            Exception exception)
        {
            _callback.Invoke(command, exception);
            return Task.CompletedTask;
        }
    }
}