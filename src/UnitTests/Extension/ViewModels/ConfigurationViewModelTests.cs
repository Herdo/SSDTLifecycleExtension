using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels
{
    using System;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.ViewModels;

    [TestFixture]
    public class ConfigurationViewModelTests
    {
        [Test]
        public void Constructor_ArgumentNullException_Project()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ConfigurationViewModel(null,
                                                                                  null,
                                                                                  null,
                                                                                  null,
                                                                                  null,
                                                                                  null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ConfigurationService()
        {
            // Arrange
            var project = new SqlProject("", "", "");

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ConfigurationViewModel(project,
                                                                                  null,
                                                                                  null,
                                                                                  null,
                                                                                  null,
                                                                                  null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_FileSystemAccess()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ConfigurationViewModel(project,
                                                                                  csMock,
                                                                                  null,
                                                                                  null,
                                                                                  null,
                                                                                  null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ScaffoldingService()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ConfigurationViewModel(project,
                                                                                  csMock,
                                                                                  fsaMock,
                                                                                  null,
                                                                                  null,
                                                                                  null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ScriptCreationService()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ConfigurationViewModel(project,
                                                                                  csMock,
                                                                                  fsaMock,
                                                                                  ssMock,
                                                                                  null,
                                                                                  null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ConfigurationViewModel(project,
                                                                                  csMock,
                                                                                  fsaMock,
                                                                                  ssMock,
                                                                                  scsMock,
                                                                                  null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_CorrectInitialization()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();

            // Act
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock);

            // Assert
            Assert.IsNull(vm.Model);
            Assert.IsNotNull(vm.BrowsePublishProfileCommand);
            Assert.IsNotNull(vm.ResetConfigurationToDefaultCommand);
            Assert.IsNotNull(vm.SaveConfigurationCommand);
        }
    }
}