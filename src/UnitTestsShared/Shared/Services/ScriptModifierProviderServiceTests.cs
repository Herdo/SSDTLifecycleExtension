namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class ScriptModifierProviderServiceTests
{
    [Test]
    public void GetScriptModifiers_NoModifiers()
    {
        // Arrange
        var smfMock = new Mock<IScriptModifierFactory>();
        IScriptModifierProviderService service = new ScriptModifierProviderService(smfMock.Object);
        var config = new ConfigurationModel
        {
            CommentOutUnnamedDefaultConstraintDrops = false,
            ReplaceUnnamedDefaultConstraintDrops = false,
            CustomHeader = null,
            CustomFooter = null,
            TrackDacpacVersion = false
        };

        // Act
        var modifiers = service.GetScriptModifiers(config);

        // Assert
        modifiers.Should().NotBeNull();
        modifiers.Should().BeEmpty();
        smfMock.Verify(m => m.CreateScriptModifier(It.IsAny<ScriptModifier>()), Times.Never);
    }

    [Test]
    public void GetScriptModifiers_AllModifiers()
    {
        // Arrange
        var sm1 = Mock.Of<IScriptModifier>();
        var sm2 = Mock.Of<IScriptModifier>();
        var sm3 = Mock.Of<IScriptModifier>();
        var sm4 = Mock.Of<IScriptModifier>();
        var sm5 = Mock.Of<IScriptModifier>();
        var sm6 = Mock.Of<IScriptModifier>();
        var smfMock = new Mock<IScriptModifierFactory>();
        smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops)).Returns(sm1);
        smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops)).Returns(sm2);
        smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.AddCustomHeader)).Returns(sm3);
        smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.AddCustomFooter)).Returns(sm4);
        smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.TrackDacpacVersion)).Returns(sm5);
        smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.RemoveSqlCmdStatements)).Returns(sm6);
        IScriptModifierProviderService service = new ScriptModifierProviderService(smfMock.Object);
        var config = new ConfigurationModel
        {
            CommentOutUnnamedDefaultConstraintDrops = true,
            ReplaceUnnamedDefaultConstraintDrops = true,
            CustomHeader = "foo",
            CustomFooter = "bar",
            TrackDacpacVersion = true,
            RemoveSqlCmdStatements = true
        };

        // Act
        var modifiers = service.GetScriptModifiers(config);

        // Assert
        modifiers.Should().NotBeNull();
        modifiers.Should().HaveCount(6);
        smfMock.Verify(m => m.CreateScriptModifier(It.IsAny<ScriptModifier>()), Times.Exactly(6));
        modifiers[ScriptModifier.CommentOutUnnamedDefaultConstraintDrops].Should().BeSameAs(sm1);
        modifiers[ScriptModifier.ReplaceUnnamedDefaultConstraintDrops].Should().BeSameAs(sm2);
        modifiers[ScriptModifier.AddCustomHeader].Should().BeSameAs(sm3);
        modifiers[ScriptModifier.AddCustomFooter].Should().BeSameAs(sm4);
        modifiers[ScriptModifier.TrackDacpacVersion].Should().BeSameAs(sm5);
        modifiers[ScriptModifier.RemoveSqlCmdStatements].Should().BeSameAs(sm6);
    }
}
