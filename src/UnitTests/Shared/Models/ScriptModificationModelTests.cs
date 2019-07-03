using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Models
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;

    [TestFixture]
    public class ScriptModificationModelTests
    {
        [Test]
        public void Constructor_ArgumentNullException_InitialScript()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModificationModel(null, null, null, null, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Project()
        {
            // Arrange
            var initialScript = "script";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModificationModel(initialScript, null, null, null, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Configuration()
        {
            // Arrange
            var initialScript = "script";
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModificationModel(initialScript, project, null, null, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Paths()
        {
            // Arrange
            var initialScript = "script";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModificationModel(initialScript, project, configuration, null, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_PreviousVersion()
        {
            // Arrange
            var initialScript = "script";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptModificationModel(initialScript, project, configuration, paths, null, false));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_CorrectInitialization()
        {
            // Arrange
            var initialScript = "script";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");
            var previousVersion = new Version(1, 2, 0);

            // Act
            var model = new ScriptModificationModel(initialScript, project, configuration, paths, previousVersion, true);

            // Assert
            Assert.AreEqual(initialScript, model.CurrentScript);
            Assert.AreSame(project, model.Project);
            Assert.AreSame(configuration, model.Configuration);
            Assert.AreSame(paths, model.Paths);
            Assert.AreSame(previousVersion, model.PreviousVersion);
            Assert.IsTrue(model.CreateLatest);
        }

        [Test]
        public void CurrentScript_Set_ArgumentNullException_WhenSetTotNull()
        {
            // Arrange
            var initialScript = "script";
            var project = new SqlProject("a", "b", "c");
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("p", "a", "b", "c", "d", "e", "f");
            var previousVersion = new Version(1, 2, 0);
            var model = new ScriptModificationModel(initialScript, project, configuration, paths, previousVersion, true);

            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => model.CurrentScript = null);
        }
    }
}