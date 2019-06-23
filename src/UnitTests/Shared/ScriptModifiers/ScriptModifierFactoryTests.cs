using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Enums;
    using SSDTLifecycleExtension.Shared.Contracts.Factories;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

    [TestFixture]
    public class ScriptModifierFactoryTests
    {
        [Test]
        public void Constructor_ArgumentNullException_DacAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModifierFactory(null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModifierFactory(daMock, null));
        }

        [Test]
        [TestCase(ScriptModifier.Undefined)]
        public void CreateScriptModifier_ArgumentOutOfRangeException(ScriptModifier scriptModifier)
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IScriptModifierFactory f = new ScriptModifierFactory(daMock, loggerMock);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => f.CreateScriptModifier(scriptModifier));
        }

        [Test]
        [TestCase(ScriptModifier.AddCustomHeader, typeof(AddCustomHeaderModifier))]
        [TestCase(ScriptModifier.AddCustomFooter, typeof(AddCustomFooterModifier))]
        [TestCase(ScriptModifier.TrackDacpacVersion, typeof(TrackDacpacVersionModifier))]
        [TestCase(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops, typeof(CommentOutUnnamedDefaultConstraintDropsModifier))]
        [TestCase(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops, typeof(ReplaceUnnamedDefaultConstraintDropsModifier))]
        public void CreateScriptModifier_CorrectCreation(ScriptModifier scriptModifier,
                                                         Type implementingType)
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IScriptModifierFactory f = new ScriptModifierFactory(daMock, loggerMock);

            // Act
            var modifier = f.CreateScriptModifier(scriptModifier);

            // Assert
            Assert.IsNotNull(modifier);
            Assert.IsInstanceOf(implementingType, modifier);
        }
    }
}