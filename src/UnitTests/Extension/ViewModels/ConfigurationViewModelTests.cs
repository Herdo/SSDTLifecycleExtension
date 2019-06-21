using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Contracts.Services;
    using SSDTLifecycleExtension.Shared.Models;
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

        [Test]
        public async Task InitializeAsync_Successfully_Async()
        {
            // Arrange
            var model = ConfigurationModel.GetDefault();
            var project = new SqlProject("", "", "");
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project)).ReturnsAsync(model);
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock.Object,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock);

            // Act
            var result = await vm.InitializeAsync();

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(vm.Model);
            Assert.AreNotSame(model, vm.Model);
            Assert.AreEqual(model, vm.Model);
        }

        [Test]
        public void Model_NoPropertyChangedForSameInstance()
        {
            // Arrange
            var model = ConfigurationModel.GetDefault();
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock)
            {
                Model = model
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
            vm.Model = model;

            // Assert
            Assert.IsNull(invokedSender);
            Assert.IsNull(invokedProperty);
        }

        [Test]
        public void Model_PropertyChangedForDifferentInstance()
        {
            // Arrange
            var model1 = ConfigurationModel.GetDefault();
            var model2 = ConfigurationModel.GetDefault();
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock)
            {
                Model = model1
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
            vm.Model = model2;

            // Assert
            Assert.AreSame(vm, invokedSender);
            Assert.AreEqual(nameof(ConfigurationViewModel.Model), invokedProperty);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void BrowsePublishProfile_CanExecute(bool setModel, bool expectedCanExecute)
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock);
            if (setModel)
                vm.Model = ConfigurationModel.GetDefault();

            // Act
            var canExecute = vm.BrowsePublishProfileCommand.CanExecute(null);

            // Assert
            Assert.AreEqual(expectedCanExecute, canExecute);
        }

        [Test]
        [TestCase(null, null, TestName = "Don't overwrite null")]
        [TestCase("TestProfile.publish.xml", "TestProfile.publish.xml", TestName = "Don't overwrite previous value")]
        public void BrowsePublishProfile_Execute_NoFileSelected(string previousPublishProfilePath, string expectedPublishProfilePath)
        {
            // Arrange
            var project = new SqlProject("", @"C:\Temp\TestProject\TestProject.sqlproj", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.BrowseForFile(".publish.xml", "Publish Profile (*.publish.xml)|*.publish.xml"))
                   .Returns(null as string);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock.Object,
                                                ssMock,
                                                scsMock,
                                                loggerMock)
            {
                Model = ConfigurationModel.GetDefault()
            };
            vm.Model.PublishProfilePath = previousPublishProfilePath;

            // Act
            vm.BrowsePublishProfileCommand.Execute(null);

            // Assert
            Assert.AreEqual(expectedPublishProfilePath, vm.Model.PublishProfilePath);
        }

        [Test]
        [TestCase(null, @"C:\Temp\TestProject\NewTestProfile.publish.xml", "NewTestProfile.publish.xml", TestName = "Overwrite null")]
        [TestCase("TestProfile.publish.xml", @"C:\Temp\TestProject\NewTestProfile.publish.xml", "NewTestProfile.publish.xml", TestName = "Overwrite previous value")]
        [TestCase("TestProfile.publish.xml", @"C:\Temp\NewTestProfile.publish.xml", @"../NewTestProfile.publish.xml", TestName = "Overwrite previous value with different directory")]
        public void BrowsePublishProfile_Execute_FileSelected(string previousPublishProfilePath, string selectedAbsolutePath, string expectedPublishProfilePath)
        {
            // Arrange
            var project = new SqlProject("", @"C:\Temp\TestProject\TestProject.sqlproj", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = new Mock<IFileSystemAccess>();
            fsaMock.Setup(m => m.BrowseForFile(".publish.xml", "Publish Profile (*.publish.xml)|*.publish.xml"))
                   .Returns(selectedAbsolutePath);
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock.Object,
                                                ssMock,
                                                scsMock,
                                                loggerMock)
            {
                Model = ConfigurationModel.GetDefault()
            };
            vm.Model.PublishProfilePath = previousPublishProfilePath;

            // Act
            vm.BrowsePublishProfileCommand.Execute(null);

            // Assert
            Assert.AreEqual(expectedPublishProfilePath, vm.Model.PublishProfilePath);
        }

        [Test]
        public void ResetConfigurationToDefault_CanExecute_Always()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock);

            // Act
            var canExecute = vm.ResetConfigurationToDefaultCommand.CanExecute(null);

            // Assert
            Assert.IsTrue(canExecute);
        }

        [Test]
        public void ResetConfigurationToDefault_Execute()
        {
            // Arrange
            var defaultModel = ConfigurationModel.GetDefault();
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock);
            object invokedSender = null;
            string invokedProperty = null;
            var errorChangedSenderList = new List<object>();
            var errorChangedPropertyList = new List<string>();
            ConfigurationModel propertyChangedModel = null;
            vm.PropertyChanged += (sender,
                                   args) =>
            {
                invokedSender = sender;
                invokedProperty = args?.PropertyName;
                if (vm.Model == null)
                    return;

                propertyChangedModel = vm.Model;
                propertyChangedModel.ErrorsChanged += (errorSender,
                                                       errorPropertyArgs) =>
                {
                    if (errorSender != null)
                        errorChangedSenderList.Add(errorSender);
                    if (errorPropertyArgs?.PropertyName != null)
                        errorChangedPropertyList.Add(errorPropertyArgs.PropertyName);
                };
            };

            // Act
            vm.ResetConfigurationToDefaultCommand.Execute(null);

            // Assert
            Assert.AreSame(vm, invokedSender);
            Assert.AreEqual(nameof(ConfigurationViewModel.Model), invokedProperty);
            Assert.AreSame(vm.Model, propertyChangedModel);
            Assert.AreEqual(defaultModel, vm.Model);
            Assert.IsTrue(errorChangedSenderList.Count > 0);
            Assert.IsTrue(errorChangedPropertyList.Count > 0);
            Assert.AreEqual(errorChangedSenderList.Count, errorChangedPropertyList.Count);
        }

        [Test]
        public void SaveConfiguration_CanExecute_NotWhenModelIsNull()
        {
            // Arrange
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock);

            // Act
            var canExecute = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void SaveConfiguration_CanExecute_NotWhenModelHasErrors()
        {
            // Arrange
            var model = ConfigurationModel.GetDefault();
            model.ReplaceUnnamedDefaultConstraintDrops = true;
            model.CommentOutUnnamedDefaultConstraintDrops = true;
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock)
            {
                Model = model
            };

            // Act
            var canExecute = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void SaveConfiguration_CanExecute_NotWhenScaffoldingIsInProgress()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "foobar",
                ReplaceUnnamedDefaultConstraintDrops = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                PublishProfilePath = "Test.publish.xml",
                VersionPattern = "1.2.3.4",
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "awesome header",
                CustomFooter = "lame footer",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = new Mock<IScaffoldingService>();
            ssMock.SetupGet(m => m.IsScaffolding).Returns(true);
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock.Object,
                                                scsMock,
                                                loggerMock)
            {
                Model = model
            };

            // Act
            var canExecute = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void SaveConfiguration_CanExecute_YesAfterScaffoldingCompleted()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "foobar",
                ReplaceUnnamedDefaultConstraintDrops = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                PublishProfilePath = "Test.publish.xml",
                VersionPattern = "1.2.3.4",
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "awesome header",
                CustomFooter = "lame footer",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = new Mock<IScaffoldingService>();
            ssMock.SetupGet(m => m.IsScaffolding).Returns(true);
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock.Object,
                                                scsMock,
                                                loggerMock)
            {
                Model = model
            };

            // Act
            var canExecuteDuringScaffolding = vm.SaveConfigurationCommand.CanExecute();
            ssMock.SetupGet(m => m.IsScaffolding).Returns(false);
            ssMock.Raise(service => service.IsScaffoldingChanged += null, EventArgs.Empty);
            var canExecuteAfterScaffolding = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsFalse(canExecuteDuringScaffolding);
            Assert.IsTrue(canExecuteAfterScaffolding);
        }

        [Test]
        public void SaveConfiguration_CanExecute_NotWhenScriptCreationIsInProgress()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "foobar",
                ReplaceUnnamedDefaultConstraintDrops = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                PublishProfilePath = "Test.publish.xml",
                VersionPattern = "1.2.3.4",
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "awesome header",
                CustomFooter = "lame footer",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating).Returns(true);
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock.Object,
                                                loggerMock)
            {
                Model = model
            };

            // Act
            var canExecute = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsFalse(canExecute);
        }

        [Test]
        public void SaveConfiguration_CanExecute_YesAfterScriptCreationCompleted()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "foobar",
                ReplaceUnnamedDefaultConstraintDrops = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                PublishProfilePath = "Test.publish.xml",
                VersionPattern = "1.2.3.4",
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "awesome header",
                CustomFooter = "lame footer",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("", "", "");
            var csMock = Mock.Of<IConfigurationService>();
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = new Mock<IScriptCreationService>();
            scsMock.SetupGet(m => m.IsCreating).Returns(true);
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock,
                                                fsaMock,
                                                ssMock,
                                                scsMock.Object,
                                                loggerMock)
            {
                Model = model
            };

            // Act
            var canExecuteDuringScriptCreation = vm.SaveConfigurationCommand.CanExecute();
            scsMock.SetupGet(m => m.IsCreating).Returns(false);
            scsMock.Raise(service => service.IsCreatingChanged += null, EventArgs.Empty);
            var canExecuteAfterScriptCreation = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsFalse(canExecuteDuringScriptCreation);
            Assert.IsTrue(canExecuteAfterScriptCreation);
        }

        [Test]
        public async Task SaveConfiguration_Execute_Async()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "foobar",
                ReplaceUnnamedDefaultConstraintDrops = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                PublishProfilePath = "Test.publish.xml",
                VersionPattern = "1.2.3.4",
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "awesome header",
                CustomFooter = "lame footer",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("", "", "");
            ConfigurationModel savedModel = null;
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()))
                  .Callback((SqlProject p, ConfigurationModel m) => savedModel = m)
                  .Returns(Task.CompletedTask);
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            var loggerMock = Mock.Of<ILogger>();
            var vm = new ConfigurationViewModel(project,
                                                csMock.Object,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock)
            {
                Model = model
            };

            // Act
            var canSaveBeforeSaving = vm.SaveConfigurationCommand.CanExecute();
            await vm.SaveConfigurationCommand.ExecuteAsync();
            var canSaveAfterSaving = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsTrue(canSaveBeforeSaving);
            Assert.IsFalse(canSaveAfterSaving);
            csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
            Assert.IsNotNull(savedModel);
            Assert.AreNotSame(model, savedModel);
            Assert.AreEqual(model, savedModel);
            Assert.AreSame(model, vm.Model);
        }

        [Test]
        public void SaveConfiguration_Execute_CallLoggerOnError()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = "foobar",
                ReplaceUnnamedDefaultConstraintDrops = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                PublishProfilePath = "Test.publish.xml",
                VersionPattern = "1.2.3.4",
                CreateDocumentationWithScriptCreation = true,
                CustomHeader = "awesome header",
                CustomFooter = "lame footer",
                BuildBeforeScriptCreation = true,
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var project = new SqlProject("", "", "");
            ConfigurationModel savedModel = null;
            var csMock = new Mock<IConfigurationService>();
            csMock.Setup(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()))
                  .Callback((SqlProject p,
                             ConfigurationModel m) => savedModel = m)
                  .ThrowsAsync(new InvalidOperationException("test exception"));
            var fsaMock = Mock.Of<IFileSystemAccess>();
            var ssMock = Mock.Of<IScaffoldingService>();
            var scsMock = Mock.Of<IScriptCreationService>();
            string loggedMessage = null;
            var loggerMock = new Mock<ILogger>();
            loggerMock.Setup(m => m.LogAsync(It.IsNotNull<string>()))
                      .Callback((string message) => loggedMessage = message)
                      .Returns(Task.CompletedTask);
            var vm = new ConfigurationViewModel(project,
                                                csMock.Object,
                                                fsaMock,
                                                ssMock,
                                                scsMock,
                                                loggerMock.Object)
            {
                Model = model
            };

            // Act
            var canSaveBeforeSaving = vm.SaveConfigurationCommand.CanExecute();
            vm.SaveConfigurationCommand.Execute(null);
            var canSaveAfterSaving = vm.SaveConfigurationCommand.CanExecute();

            // Assert
            Assert.IsTrue(canSaveBeforeSaving);
            Assert.IsTrue(canSaveAfterSaving);
            csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
            Assert.IsNotNull(savedModel);
            Assert.AreNotSame(model, savedModel);
            Assert.AreEqual(model, savedModel);
            Assert.AreSame(model, vm.Model);
            loggerMock.Verify(m => m.LogAsync(It.IsNotNull<string>()), Times.Once);
            Assert.IsNotNull(loggedMessage);
            Assert.IsTrue(loggedMessage.Contains(nameof(InvalidOperationException)));
            Assert.IsTrue(loggedMessage.Contains(nameof(ConfigurationViewModel.SaveConfigurationCommand)));
            Assert.IsTrue(loggedMessage.Contains("test exception"));
        }
    }
}