using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

    [TestFixture]
    public class AddCustomHeaderModifierTests
    {
        [Test]
        public void Modify_ArgumentNullException_Input()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.Modify(null, null, default));
        }

        [Test]
        public void Modify_ArgumentNullException_Configuration()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.Modify(input, null, default));
        }

        [Test]
        public void Modify_CustomHeaderAdded()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";
            var configuration = new ConfigurationModel
            {
                CustomHeader = "Header"
            };

            // Act
            var modified = s.Modify(input, configuration, default);

            // Assert
            Assert.IsNotNull(modified);
            Assert.AreEqual("Header\r\nfoobar", modified);
        }

        [Test]
        [TestCase(null,TestName = "<null>")]
        [TestCase("", TestName = "<empty string>")]
        [TestCase("    ", TestName = "<4 whitespaces>")]
        [TestCase(" ", TestName = "<1 tabulator>")]
        public void Modify_NoLeadingNewLineWhenNullOrWhiteSpace(string customHeader)
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";
            var configuration = new ConfigurationModel
            {
                CustomHeader = customHeader
            };

            // Act
            var modified = s.Modify(input, configuration, default);

            // Assert
            Assert.IsNotNull(modified);
            Assert.AreEqual("foobar", modified);
        }
    }
}