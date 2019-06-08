using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

    [TestFixture]
    public class AddCustomFooterModifierTests
    {
        [Test]
        public void Modify_ArgumentNullException_Input()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.Modify(null, null, null, default));
        }

        [Test]
        public void Modify_ArgumentNullException_Project()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.Modify(input, null, null, default));
        }

        [Test]
        public void Modify_ArgumentNullException_Configuration()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.Modify(input, project, null, default));
        }

        [Test]
        public void Modify_CustomFooterAdded()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel
            {
                CustomFooter = "footer"
            };

            // Act
            var modified = s.Modify(input, project, configuration, default);

            // Assert
            Assert.IsNotNull(modified);
            Assert.AreEqual("foobar\r\nfooter", modified);
        }

        [Test]
        [TestCase(null,TestName = "<null>")]
        [TestCase("", TestName = "<empty string>")]
        [TestCase("    ", TestName = "<4 whitespaces>")]
        [TestCase(" ", TestName = "<1 tabulator>")]
        public void Modify_NoTrailingNewLineWhenNullOrWhiteSpace(string customFooter)
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel
            {
                CustomFooter = customFooter
            };

            // Act
            var modified = s.Modify(input, project, configuration, default);

            // Assert
            Assert.IsNotNull(modified);
            Assert.AreEqual("foobar", modified);
        }
    }
}