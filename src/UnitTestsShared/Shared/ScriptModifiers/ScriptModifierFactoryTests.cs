namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers;

[TestFixture]
public class ScriptModifierFactoryTests
{
    [Test]
    [TestCase(ScriptModifier.Undefined)]
    public void CreateScriptModifier_ArgumentOutOfRangeException(ScriptModifier scriptModifier)
    {
        // Arrange
        var drMock = Mock.Of<IDependencyResolver>();
        IScriptModifierFactory f = new ScriptModifierFactory(drMock);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => f.CreateScriptModifier(scriptModifier));
    }

    [Test]
    [TestCase(ScriptModifier.AddCustomHeader, typeof(AddCustomHeaderModifier))]
    [TestCase(ScriptModifier.AddCustomFooter, typeof(AddCustomFooterModifier))]
    [TestCase(ScriptModifier.TrackDacpacVersion, typeof(TrackDacpacVersionModifier))]
    [TestCase(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops, typeof(CommentOutUnnamedDefaultConstraintDropsModifier))]
    [TestCase(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops, typeof(ReplaceUnnamedDefaultConstraintDropsModifier))]
    [TestCase(ScriptModifier.RemoveSqlCmdStatements, typeof(RemoveSqlCmdStatementsModifier))]
    public void CreateScriptModifier_CorrectCreation(ScriptModifier scriptModifier,
                                                     Type implementingType)
    {
        // Arrange
        var daMock = Mock.Of<IDacAccess>();
        var loggerMock = Mock.Of<ILogger>();
        var drMock = new Mock<IDependencyResolver>();
        drMock.Setup(m => m.Get<AddCustomHeaderModifier>()).Returns(new AddCustomHeaderModifier());
        drMock.Setup(m => m.Get<AddCustomFooterModifier>()).Returns(new AddCustomFooterModifier());
        drMock.Setup(m => m.Get<TrackDacpacVersionModifier>()).Returns(new TrackDacpacVersionModifier());
        drMock.Setup(m => m.Get<CommentOutUnnamedDefaultConstraintDropsModifier>()).Returns(new CommentOutUnnamedDefaultConstraintDropsModifier());
        drMock.Setup(m => m.Get<ReplaceUnnamedDefaultConstraintDropsModifier>()).Returns(new ReplaceUnnamedDefaultConstraintDropsModifier(daMock, loggerMock));
        drMock.Setup(m => m.Get<RemoveSqlCmdStatementsModifier>()).Returns(new RemoveSqlCmdStatementsModifier());
        IScriptModifierFactory f = new ScriptModifierFactory(drMock.Object);

        // Act
        var modifier = f.CreateScriptModifier(scriptModifier);

        // Assert
        modifier.Should().BeOfType(implementingType);
    }
}