using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Services
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.Enums;
    using SSDTLifecycleExtension.Shared.Contracts.Factories;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.Services;

    [TestFixture]
    public class ScriptModifierProviderServiceTests
    {
        [Test]
        public void Constructor_ArgumentNullException_ScriptModifierFactory()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModifierProviderService(null));
        }

        [Test]
        public void GetScriptModifiers_ArgumentNullException_Configuration()
        {
            // Arrange
            var smfMock = new Mock<IScriptModifierFactory>();
            IScriptModifierProviderService service = new ScriptModifierProviderService(smfMock.Object);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => service.GetScriptModifiers(null));
        }

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
            Assert.IsNotNull(modifiers);
            Assert.AreEqual(0, modifiers.Count);
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
            var smfMock = new Mock<IScriptModifierFactory>();
            smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops)).Returns(sm1);
            smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops)).Returns(sm2);
            smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.AddCustomHeader)).Returns(sm3);
            smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.AddCustomFooter)).Returns(sm4);
            smfMock.Setup(m => m.CreateScriptModifier(ScriptModifier.TrackDacpacVersion)).Returns(sm5);
            IScriptModifierProviderService service = new ScriptModifierProviderService(smfMock.Object);
            var config = new ConfigurationModel
            {
                CommentOutUnnamedDefaultConstraintDrops = true,
                ReplaceUnnamedDefaultConstraintDrops = true,
                CustomHeader = "foo",
                CustomFooter = "bar",
                TrackDacpacVersion = true
            };

            // Act
            var modifiers = service.GetScriptModifiers(config);

            // Assert
            Assert.IsNotNull(modifiers);
            Assert.AreEqual(5, modifiers.Count);
            smfMock.Verify(m => m.CreateScriptModifier(It.IsAny<ScriptModifier>()), Times.Exactly(5));
            Assert.AreSame(modifiers[ScriptModifier.CommentOutUnnamedDefaultConstraintDrops], sm1);
            Assert.AreSame(modifiers[ScriptModifier.ReplaceUnnamedDefaultConstraintDrops], sm2);
            Assert.AreSame(modifiers[ScriptModifier.AddCustomHeader], sm3);
            Assert.AreSame(modifiers[ScriptModifier.AddCustomFooter], sm4);
            Assert.AreSame(modifiers[ScriptModifier.TrackDacpacVersion], sm5);
        }
    }
}