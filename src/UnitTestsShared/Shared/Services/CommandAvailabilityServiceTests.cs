namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class CommandAvailabilityServiceTests
{
    [Test]
    public void Constructor_ArgumentNullException_VisualStudioAccess()
    {
        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new CommandAvailabilityService(null, null, null));
    }

    [Test]
    public void Constructor_ArgumentNullException_ScaffoldingService()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new CommandAvailabilityService(vsaMock, null, null));
    }

    [Test]
    public void Constructor_ArgumentNullException_ScriptCreationService()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var scaffoldingMock = Mock.Of<IScaffoldingService>();

        // Act & Assert
        // ReSharper disable once ObjectCreationAsStatement
        Assert.Throws<ArgumentNullException>(() => new CommandAvailabilityService(vsaMock, scaffoldingMock, null));
    }

    [Test]
    public void HandleCommandAvailability_ArgumentNullException_SetVisible()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var scaffoldingMock = Mock.Of<IScaffoldingService>();
        var scriptCreationMock = Mock.Of<IScriptCreationService>();
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock, scaffoldingMock, scriptCreationMock);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.HandleCommandAvailability(null, null));
    }

    [Test]
    public void HandleCommandAvailability_ArgumentNullException_SetEnabled()
    {
        // Arrange
        var vsaMock = Mock.Of<IVisualStudioAccess>();
        var scaffoldingMock = Mock.Of<IScaffoldingService>();
        var scriptCreationMock = Mock.Of<IScriptCreationService>();
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock, scaffoldingMock, scriptCreationMock);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => service.HandleCommandAvailability(b => { }, null));
    }

    [Test]
    public void HandleCommandAvailability_NoProjectSelected()
    {
        // Arrange
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.GetSelectedProjectKind()).Returns(Guid.Empty);
        var scaffoldingMock = Mock.Of<IScaffoldingService>();
        var scriptCreationMock = Mock.Of<IScriptCreationService>();
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock.Object, scaffoldingMock, scriptCreationMock);
        bool? visible = null;
        bool? enabled = null;

        // Act
        service.HandleCommandAvailability(b => visible = b, b => enabled = b);

        // Assert
        Assert.IsNull(visible);
        Assert.IsNull(enabled);
    }

    [Test]
    public void HandleCommandAvailability_NoSqlProjectSelected()
    {
        // Arrange
        var vsaMock = new Mock<IVisualStudioAccess>();
        vsaMock.Setup(m => m.GetSelectedProjectKind()).Returns(Guid.Parse("{250BC36C-9B42-4736-BBAB-C3B938A26F8A}"));
        var scaffoldingMock = Mock.Of<IScaffoldingService>();
        var scriptCreationMock = Mock.Of<IScriptCreationService>();
        ICommandAvailabilityService service = new CommandAvailabilityService(vsaMock.Object, scaffoldingMock, scriptCreationMock);
        bool? visible = null;
        bool? enabled = null;

        // Act
        service.HandleCommandAvailability(b => visible = b, b => enabled = b);

        // Assert
        Assert.IsFalse(visible);
        Assert.IsFalse(enabled);
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
        vsaMock.Setup(m => m.GetSelectedProjectKind()).Returns(Guid.Parse("{00d1a9c2-b5f0-4af3-8072-f6c62b433612}"));
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
        Assert.IsTrue(visible);
        Assert.AreEqual(expectedEnabled, enabled);
    }
}