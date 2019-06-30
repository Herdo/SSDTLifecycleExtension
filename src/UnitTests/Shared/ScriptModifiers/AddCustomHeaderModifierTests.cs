using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using System.Threading.Tasks;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

    [TestFixture]
    public class AddCustomHeaderModifierTests
    {
        [Test]
        public void Modify_ArgumentNullException_Model()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => s.ModifyAsync(null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task Modify_CustomHeaderAdded_Async()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            project.ProjectProperties.DacVersion = new Version(1, 1, 0);
            var configuration = new ConfigurationModel
            {
                CustomHeader = "Header"
            };
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");
            var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 0, 0), false);

            // Act
            await s.ModifyAsync(model);

            // Assert
            Assert.IsNotNull(model.CurrentScript);
            Assert.AreEqual("Header\r\nfoobar", model.CurrentScript);
        }

        [Test]
        [TestCase(null,TestName = "<null>")]
        [TestCase("", TestName = "<empty string>")]
        [TestCase("    ", TestName = "<4 whitespaces>")]
        [TestCase(" ", TestName = "<1 tabulator>")]
        public async Task Modify_NoLeadingNewLineWhenNullOrWhiteSpace_Async(string customHeader)
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel
            {
                CustomHeader = customHeader
            };
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");
            var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 0, 0), false);

            // Act
            await s.ModifyAsync(model);

            // Assert
            Assert.IsNotNull(model.CurrentScript);
            Assert.AreEqual("foobar", model.CurrentScript);
        }

        [Test]
        public async Task Modify_ReplaceSpecialKeyword_PreviousVersion_Async()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            project.ProjectProperties.DacVersion = new Version(1, 3 , 0);
            var configuration = new ConfigurationModel
            {
                CustomHeader = "Script base version: {PREVIOUS_VERSION}"
            };
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");
            var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

            // Act
            await s.ModifyAsync(model);

            // Assert
            Assert.IsNotNull(model.CurrentScript);
            Assert.AreEqual("Script base version: 1.2.0\r\nfoobar", model.CurrentScript);
        }

        [Test]
        public async Task Modify_ReplaceSpecialKeyword_NextVersion_Async()
        {
            // Arrange
            IScriptModifier s = new AddCustomHeaderModifier();
            const string input = "foobar";
            var project = new SqlProject("a", "b", "c");
            project.ProjectProperties.DacVersion = new Version(1, 3, 0);
            var configuration = new ConfigurationModel
            {
                CustomHeader = "Script target version: {NEXT_VERSION}"
            };
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");
            var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

            // Act
            await s.ModifyAsync(model);

            // Assert
            Assert.IsNotNull(model.CurrentScript);
            Assert.AreEqual("Script target version: 1.3.0\r\nfoobar", model.CurrentScript);
        }
    }
}