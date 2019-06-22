using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using System.Threading.Tasks;
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
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => s.ModifyAsync(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Project()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => s.ModifyAsync(input, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Configuration()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => s.ModifyAsync(input, project, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Paths()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => s.ModifyAsync(input, project, configuration, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task Modify_CustomFooterAdded_Async()
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel
            {
                CustomFooter = "footer"
            };
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");

            // Act
            var modified = await s.ModifyAsync(input, project, configuration, paths);

            // Assert
            Assert.IsNotNull(modified);
            Assert.AreEqual("foobar\r\nfooter", modified);
        }

        [Test]
        [TestCase(null,TestName = "<null>")]
        [TestCase("", TestName = "<empty string>")]
        [TestCase("    ", TestName = "<4 whitespaces>")]
        [TestCase(" ", TestName = "<1 tabulator>")]
        public async Task Modify_NoTrailingNewLineWhenNullOrWhiteSpace_Async(string customFooter)
        {
            // Arrange
            IScriptModifier s = new AddCustomFooterModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel
            {
                CustomFooter = customFooter
            };
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");

            // Act
            var modified = await s.ModifyAsync(input, project, configuration, paths);

            // Assert
            Assert.IsNotNull(modified);
            Assert.AreEqual("foobar", modified);
        }
    }
}