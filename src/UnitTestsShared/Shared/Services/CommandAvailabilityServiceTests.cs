namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class CommandAvailabilityServiceTests
{
    [Test]
    public void HandleCommandAvailability_NoProjectSelected()
    {
        // Arrange
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.IsSelectedProjectOfKindAsync(It.IsAny<string>())).ReturnsAsync(false);
        var scaffoldingMock = Mock.Of<IScaffoldingService>();
        var scriptCreationMock = Mock.Of<IScriptCreationService>();
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock.Object, scaffoldingMock, scriptCreationMock);
        bool? visible = null;
        bool? enabled = null;

        // Act
        service.HandleCommandAvailability(b => visible = b, b => enabled = b);

        // Assert
        visible.Should().BeFalse();
        enabled.Should().BeFalse();
    }

    [Test]
    public void HandleCommandAvailability_NoSqlProjectSelected()
    {
        // Arrange
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.IsSelectedProjectOfKindAsync("250BC36C-9B42-4736-BBAB-C3B938A26F8A")).ReturnsAsync(false);
        var scaffoldingMock = Mock.Of<IScaffoldingService>();
        var scriptCreationMock = Mock.Of<IScriptCreationService>();
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock.Object, scaffoldingMock, scriptCreationMock);
        bool? visible = null;
        bool? enabled = null;

        // Act
        service.HandleCommandAvailability(b => visible = b, b => enabled = b);

        // Assert
        visible.Should().BeFalse();
        enabled.Should().BeFalse();
    }

    [Test]
    [TestCase(true, true, false)]
    [TestCase(true, false, false)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    public void HandleCommandAvailability_SqlProjectSelected(bool isScaffolding, bool isCreatingScript, bool expectedEnabled)
    {
        // Arrange
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.IsSelectedProjectOfKindAsync("00d1a9c2-b5f0-4af3-8072-f6c62b433612")).ReturnsAsync(true);
        var scaffoldingMock = new Mock<IScaffoldingService>();
        scaffoldingMock.SetupGet(m => m.IsScaffolding).Returns(isScaffolding);
        var scriptCreationMock = new Mock<IScriptCreationService>();
        scriptCreationMock.SetupGet(m => m.IsCreating).Returns(isCreatingScript);
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock.Object, scaffoldingMock.Object, scriptCreationMock.Object);
        bool? visible = null;
        bool? enabled = null;

        // Act
        service.HandleCommandAvailability(b => visible = b, b => enabled = b);

        // Assert
        visible.Should().BeTrue();
        enabled.Should().Be(expectedEnabled);
    }
}