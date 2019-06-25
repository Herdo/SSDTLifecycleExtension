using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Models
{
    using System;
    using System.Threading.Tasks;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;

    [TestFixture]
    public class ScriptCreationStateModelTests
    {
        [Test]
        public void Constructor_ArgumentNullException_Project()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationStateModel(null, null, null, true, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Configuration()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationStateModel(project, null, null, true, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_PreviousVersion()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationStateModel(project, configuration, null, true, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_HandleWorkInProgressChanged()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 0);

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationStateModel(project, configuration, previousVersion, true, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_CorrectInitialization()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 0);
            // ReSharper disable once ConvertToLocalFunction
            Func<bool, Task> changeHandler = b => Task.CompletedTask;

            // Act
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, changeHandler);

            // Assert
            Assert.AreSame(project, model.Project);
            Assert.AreSame(configuration, model.Configuration);
            Assert.AreSame(previousVersion, model.PreviousVersion);
            Assert.IsTrue(model.CreateLatest);
            Assert.AreSame(changeHandler, model.HandleWorkInProgressChanged);
        }

        [Test]
        public void FormattedTargetVersion_Get_Set()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 0);
            // ReSharper disable once ConvertToLocalFunction
            Func<bool, Task> changeHandler = b => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, changeHandler);
            var formattedVersion = new Version(1, 0, 0);

            // Act
            model.FormattedTargetVersion = formattedVersion;

            // Assert
            Assert.AreSame(formattedVersion, model.FormattedTargetVersion);
        }

        [Test]
        public void Paths_Get_Set()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var configuration = ConfigurationModel.GetDefault();
            var previousVersion = new Version(1, 0);
            // ReSharper disable once ConvertToLocalFunction
            Func<bool, Task> changeHandler = b => Task.CompletedTask;
            var model = new ScriptCreationStateModel(project, configuration, previousVersion, true, changeHandler);
            var paths = new PathCollection("a", "b", "c", "d", "e", "f");

            // Act
            model.Paths = paths;

            // Assert
            Assert.AreSame(paths, model.Paths);
        }
    }
}