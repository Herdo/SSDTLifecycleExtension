namespace SSDTLifecycleExtension.UnitTests.Extension.Commands;

[TestFixture]
public class BaseCommandTests
{
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
        cmd.Should().NotBeNull();
        var registeredCommand = cs.FindCommand(commandId);
        registeredCommand.Should().NotBeNull();
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
        var registeredCommand = cs.FindCommand(commandId);
        registeredCommand.Visible.Should().BeTrue();
        registeredCommand.Enabled.Should().BeTrue();
        var status = registeredCommand.OleStatus;

        // Assert
        cmd.Should().NotBeNull();
        status.Should().NotBe(0);
        handleCommandAvailabilityCalled.Should().BeTrue();
        registeredCommand.Visible.Should().BeFalse();
        registeredCommand.Enabled.Should().BeFalse();
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
        cmd.Should().NotBeNull();
        var invoked = cs.GlobalInvoke(commandId);
        invoked.Should().BeTrue();
        executeSender.Should().NotBeNull();
        executeArgs.Should().NotBeNull();
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