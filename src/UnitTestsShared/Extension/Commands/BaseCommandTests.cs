namespace SSDTLifecycleExtension.UnitTests.Extension.Commands;

[TestFixture]
public class BaseCommandTests
{
    [Test]
    public void Constructor_ArgumentNullException_CommandService()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new BaseCommandTestImplementation(null, null, 0, Guid.Empty));
    }

    [Test]
    public void Constructor_ArgumentNullException_CommandAvailabilityService()
    {
        // Arrange
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new BaseCommandTestImplementation(cs, null, 0, Guid.Empty));
    }

    [Test]
    public void Constructor_CommandAddedSuccessfully()
    {
        // Arrange
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var casMock = Mock.Of<ICommandAvailabilityService>();
        const int commandIdInt = 4711;
        var commandSet = Guid.Parse("{110031CC-14A1-44FA-83D1-D970918981AC}");
        var commandId = new CommandID(commandSet, commandIdInt);

        // Act
        var cmd = new BaseCommandTestImplementation(cs,
                                                    casMock,
                                                    commandIdInt,
                                                    commandSet);

        // Assert
        Assert.IsNotNull(cmd);
        var registeredCommand = cs.FindCommand(commandId);
        Assert.IsNotNull(registeredCommand);
    }

    [Test]
    public void Constructor_CommandAvailabilityChecked()
    {
        // Arrange
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var casMock = new Mock<ICommandAvailabilityService>();
        var handleCommandAvailabilityCalled = false;
        casMock.Setup(m => m.HandleCommandAvailability(It.IsNotNull<Action<bool>>(), It.IsNotNull<Action<bool>>()))
               .Callback((Action<bool> setVisibleAction,
                          Action<bool> setEnabledAction) =>
               {
                   handleCommandAvailabilityCalled = true;
                   setVisibleAction(false);
                   setEnabledAction(false);
               });
        const int commandIdInt = 4711;
        var commandSet = Guid.Parse("{110031CC-14A1-44FA-83D1-D970918981AC}");
        var commandId = new CommandID(commandSet, commandIdInt);

        // Act
        var cmd = new BaseCommandTestImplementation(cs,
                                                    casMock.Object,
                                                    commandIdInt,
                                                    commandSet);
        Assert.IsNotNull(cmd);
        var registeredCommand = cs.FindCommand(commandId);
        Assert.IsNotNull(registeredCommand);
        Assert.IsTrue(registeredCommand.Visible);
        Assert.IsTrue(registeredCommand.Enabled);
        var status = registeredCommand.OleStatus;

        // Assert
        Assert.AreNotEqual(0, status);
        Assert.IsTrue(handleCommandAvailabilityCalled);
        Assert.IsFalse(registeredCommand.Visible);
        Assert.IsFalse(registeredCommand.Enabled);
    }

    [Test]
    public void Constructor_InvokeCallsDerivedMethod()
    {
        // Arrange
        var spMock = Mock.Of<IServiceProvider>();
        var cs = new OleMenuCommandService(spMock);
        var casMock = Mock.Of<ICommandAvailabilityService>();
        const int commandIdInt = 4711;
        var commandSet = Guid.Parse("{110031CC-14A1-44FA-83D1-D970918981AC}");
        var commandId = new CommandID(commandSet, commandIdInt);
        object executeSender = null;
        EventArgs executeArgs = null;

        // Act
        var cmd = new BaseCommandTestImplementation(cs,
                                                    casMock,
                                                    commandIdInt,
                                                    commandSet)
        {
            ExecuteCalledCallback = (o,
                                     args) =>
            {
                executeSender = o;
                executeArgs = args;
            }
        };

        // Assert
        Assert.IsNotNull(cmd);
        var invoked = cs.GlobalInvoke(commandId);
        Assert.IsTrue(invoked);
        Assert.IsNotNull(executeSender);
        Assert.IsNotNull(executeArgs);
    }

    private sealed class BaseCommandTestImplementation : BaseCommand
    {
        internal Action<object, EventArgs> ExecuteCalledCallback { get; set; }

        internal BaseCommandTestImplementation(OleMenuCommandService commandService,
                                               ICommandAvailabilityService commandAvailabilityService,
                                               int commandId,
                                               Guid commandSet)
            : base(commandService, commandAvailabilityService, commandId, commandSet)
        {
        }

        protected override void Execute(object sender,
                                        EventArgs e)
        {
            ExecuteCalledCallback?.Invoke(sender, e);
        }
    }
}