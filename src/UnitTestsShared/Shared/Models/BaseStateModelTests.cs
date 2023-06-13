namespace SSDTLifecycleExtension.UnitTests.Shared.Models;

[TestFixture]
public class BaseStateModelTests
{
    [Test]
    public void Constructor_ArgumentNullException_HandleWorkInProgressChanged()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        // ReSharper disable once AssignNullToNotNullAttribute
        Assert.Throws<ArgumentNullException>(() => new BaseStateModelTestImplementation(null));
    }

    [Test]
    public void Constructor_CorrectInitialization()
    {
        // Arrange
        // ReSharper disable once ConvertToLocalFunction
        Func<bool, Task> handlerFunc = b => Task.CompletedTask;

        // Act
        var model = new BaseStateModelTestImplementation(handlerFunc);

        // Assert
        Assert.AreSame(handlerFunc, model.HandleWorkInProgressChanged);
        Assert.AreEqual(StateModelState.Initialized, model.CurrentState);
        Assert.IsNull(model.Result);
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
        Assert.AreEqual(StateModelState.PathsVerified, model.CurrentState);
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
        Assert.AreEqual(true, model.Result);
    }

    private class BaseStateModelTestImplementation : BaseStateModel
    {
        internal BaseStateModelTestImplementation([NotNull] Func<bool, Task> handleWorkInProgressChanged)
            : base(handleWorkInProgressChanged)
        {
        }
    }
}