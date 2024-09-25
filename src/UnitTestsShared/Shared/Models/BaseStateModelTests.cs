namespace SSDTLifecycleExtension.UnitTests.Shared.Models;

[TestFixture]
public class BaseStateModelTests
{
    [Test]
    public void Constructor_CorrectInitialization()
    {
        // Arrange
        // ReSharper disable once ConvertToLocalFunction
        Func<bool, Task> handlerFunc = b => Task.CompletedTask;

        // Act
        var model = new BaseStateModelTestImplementation(handlerFunc);

        // Assert
        model.HandleWorkInProgressChanged.Should().BeSameAs(handlerFunc);
        model.CurrentState.Should().Be(StateModelState.Initialized);
        model.Result.Should().BeNull();
    }

    [Test]
    public void CurrentState_Get_Set()
    {
        // Arrange
        Task HandlerFunc(bool b) => Task.CompletedTask;
        // ReSharper disable once UseObjectOrCollectionInitializer
        var model = new BaseStateModelTestImplementation(HandlerFunc);

        // Act
        model.CurrentState = StateModelState.PathsVerified;

        // Assert
        model.CurrentState.Should().Be(StateModelState.PathsVerified);
    }

    [Test]
    public void Result_Get_Set()
    {
        // Arrange
        Task HandlerFunc(bool b) => Task.CompletedTask;
        // ReSharper disable once UseObjectOrCollectionInitializer
        var model = new BaseStateModelTestImplementation(HandlerFunc);

        // Act
        model.Result = true;

        // Assert
        model.Result.Should().BeTrue();
    }

    private class BaseStateModelTestImplementation : BaseStateModel
    {
        internal BaseStateModelTestImplementation(Func<bool, Task> handleWorkInProgressChanged)
            : base(handleWorkInProgressChanged)
        {
        }
    }
}