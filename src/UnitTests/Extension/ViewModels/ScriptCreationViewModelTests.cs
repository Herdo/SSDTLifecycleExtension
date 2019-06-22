using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.ViewModels;

    [TestFixture]
    public class ScriptCreationViewModelTests
    {
        [Test]
        public void Constructor_ArgumentNullException_Project()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(null, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ConfigurationService()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, null, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ScaffoldingService()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, null, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ScriptCreationService()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_VisualStudioAccess()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, scsMock, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_CorrectInitialization()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();

            // Act
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock);

            // Assert
            Assert.IsNotNull(vm.ExistingVersions);
            Assert.AreEqual(0, vm.ExistingVersions.Count);
            Assert.IsNotNull(vm.ScaffoldDevelopmentVersionCommand);
            Assert.IsNotNull(vm.ScaffoldCurrentProductionVersionCommand);
            Assert.IsNotNull(vm.StartLatestCreationCommand);
            Assert.IsNotNull(vm.StartVersionedCreationCommand);
        }

        [Test]
        public void SelectedBaseVersion_NoPropertyChangedForSameInstance()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var version = new VersionModel();
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock)
            {
                SelectedBaseVersion = version
            };
            object invokedSender = null;
            string invokedProperty = null;
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                invokedSender = sender;
                invokedProperty = args?.PropertyName;
            };

            // Act
            vm.SelectedBaseVersion = version;

            // Assert
            Assert.IsNull(invokedSender);
            Assert.IsNull(invokedProperty);
        }

        [Test]
        public void SelectedBaseVersion_Get_Set_PropertyChanged()
        {
            // Arrange
            var project = new SqlProject("a", "b", "c");
            var csMock = Mock.Of<IConfigurationService>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var vsaMock = Mock.Of<IVisualStudioAccess>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var version = new VersionModel();
            var vm = new ScriptCreationViewModel(project, csMock, ssMock, scsMock, vsaMock, fsaMock, loggerMock);
            object invokedSender = null;
            string invokedProperty = null;
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                invokedSender = sender;
                invokedProperty = args?.PropertyName;
            };

            // Act
            vm.SelectedBaseVersion = version;

            // Assert
            Assert.AreSame(vm, invokedSender);
            Assert.AreEqual(nameof(ScriptCreationViewModel.SelectedBaseVersion), invokedProperty);
        }
    }
}