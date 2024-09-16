namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels;

[TestFixture]
public class ConfigurationViewModelTests
{
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
        vm.Model.Should().BeNull();
        vm.BrowsePublishProfileCommand.Should().NotBeNull();
        vm.ResetConfigurationToDefaultCommand.Should().NotBeNull();
        vm.SaveConfigurationCommand.Should().NotBeNull();
        vm.OpenDocumentationCommand.Should().NotBeNull();
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
        result.Should().BeTrue();
        vm.Model.Should().NotBeNull();
        vm.Model.Should().NotBeSameAs(model);
        vm.Model.Should().Be(model);
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
        invokedSender.Should().BeNull();
        invokedProperty.Should().BeNull();
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
        invokedSender.Should().BeSameAs(vm);
        invokedProperty.Should().Be(nameof(ConfigurationViewModel.Model));
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
        canExecute.Should().Be(expectedCanExecute);
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
        vm.Model.PublishProfilePath.Should().Be(expectedPublishProfilePath);
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
        vm.Model.PublishProfilePath.Should().Be(expectedPublishProfilePath);
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
        canExecute.Should().BeTrue();
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

        var changedSenders = new List<object>();
        var changedProperties = new List<string>();
        var errorChangedSenderList = new List<object>();
        var errorChangedPropertyList = new List<string>();
        ConfigurationModel propertyChangedModel = null;
        vm.PropertyChanged += (sender, args) =>
        {
            if (sender != null)
                changedSenders.Add(sender);
            if (args?.PropertyName != null)
                changedProperties.Add(args.PropertyName);
            if (vm.Model == null)
                return;

            propertyChangedModel = vm.Model;
            propertyChangedModel.ErrorsChanged += (errorSender, errorPropertyArgs) =>
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
        changedSenders.Should().HaveCount(2);
        changedSenders[0].Should().BeSameAs(vm);
        changedSenders[1].Should().BeSameAs(vm);
        changedProperties.Should().HaveCount(2);
        changedProperties[0].Should().Be(nameof(ConfigurationViewModel.Model));
        changedProperties[1].Should().Be(nameof(ConfigurationViewModel.IsModelDirty));
        vm.Model.Should().BeSameAs(propertyChangedModel);
        vm.Model.Should().Be(defaultModel);
        errorChangedSenderList.Should().NotBeEmpty();
        errorChangedPropertyList.Should().NotBeEmpty();
        errorChangedSenderList.Should().HaveSameCount(errorChangedPropertyList);
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
                                            loggerMock)
        {
            Model = ConfigurationModel.GetDefault()
        };
        vm.Model = null;

        // Act
        var canExecute = vm.SaveConfigurationCommand.CanExecute();

        // Assert
        canExecute.Should().BeFalse();
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
        canExecute.Should().BeFalse();
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
            TrackDacpacVersion = false
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
        canExecute.Should().BeFalse();
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
            TrackDacpacVersion = false
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
        canExecuteDuringScaffolding.Should().BeFalse();
        canExecuteAfterScaffolding.Should().BeTrue();
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
            TrackDacpacVersion = false
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
        canExecute.Should().BeFalse();
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
            TrackDacpacVersion = false
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
        canExecuteDuringScriptCreation.Should().BeFalse();
        canExecuteAfterScriptCreation.Should().BeTrue();
    }

    [Test]
    public async Task SaveConfiguration_Execute_SaveFailed_Async()
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        ConfigurationModel savedModel = null;
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()))
              .Callback((SqlProject p, ConfigurationModel m) => savedModel = m)
              .ReturnsAsync(false);
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
        canSaveBeforeSaving.Should().BeTrue();
        canSaveAfterSaving.Should().BeTrue();
        csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
        savedModel.Should().NotBeNull();
        savedModel.Should().NotBeSameAs(model);
        savedModel.Should().Be(model);
        vm.Model.Should().BeSameAs(model);
    }

    [Test]
    public async Task SaveConfiguration_Execute_SaveSuccessful_Async()
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        ConfigurationModel savedModel = null;
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()))
              .Callback((SqlProject p, ConfigurationModel m) => savedModel = m)
              .ReturnsAsync(true);
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
        canSaveBeforeSaving.Should().BeTrue();
        canSaveAfterSaving.Should().BeFalse();
        csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
        savedModel.Should().NotBeNull();
        savedModel.Should().NotBeSameAs(model);
        savedModel.Should().Be(model);
        vm.Model.Should().BeSameAs(model);
    }

    [Test]
    public void SaveConfiguration_Execute_CallLoggerOnError()
    {
        // Arrange
        var testException = new InvalidOperationException("test exception");
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        ConfigurationModel savedModel = null;
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()))
              .Callback((SqlProject p, ConfigurationModel m) => savedModel = m)
              .ThrowsAsync(testException);
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        Exception loggedException = null;
        string loggedMessage = null;
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsNotNull<string>()))
                  .Callback((Exception exception, string message) =>
                  {
                      loggedException = exception;
                      loggedMessage = message;
                  })
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
        canSaveBeforeSaving.Should().BeTrue();
        canSaveAfterSaving.Should().BeTrue();
        csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
        savedModel.Should().NotBeNull();
        savedModel.Should().NotBeSameAs(model);
        savedModel.Should().Be(model);
        vm.Model.Should().BeSameAs(model);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()), Times.Once);
        loggedException.Should().BeSameAs(testException);
        loggedMessage.Should().NotBeNull();
        loggedMessage.Should().Contain(nameof(ConfigurationViewModel.SaveConfigurationCommand));
    }

    [Test]
    public void SaveConfiguration_Execute_CallLoggerOnError_DoNotThrowExceptionFromLogger()
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()))
              .ThrowsAsync(new InvalidOperationException("test exception"));
        var fsaMock = Mock.Of<IFileSystemAccess>();
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()))
                  .ThrowsAsync(new Exception("logger failed"));
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
        Assert.DoesNotThrow(() => vm.SaveConfigurationCommand.Execute(null));

        // Assert
        csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
        loggerMock.Verify(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()), Times.Once);
    }

    [Test]
    public void OpenDocumentation_CanExecute_Always()
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
        var canExecute = vm.OpenDocumentationCommand.CanExecute(null);

        // Assert
        canExecute.Should().BeTrue();
    }

    [Test]
    [TestCase(1)]
    [TestCase(null)]
    [TestCase(5.0)]
    [TestCase(StateModelState.Undefined)]
    public void OpenDocumentation_Execute_NoStringAnchor(object anchor)
    {
        // Arrange
        var project = new SqlProject("", "", "");
        var csMock = Mock.Of<IConfigurationService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ConfigurationViewModel(project,
                                            csMock,
                                            fsaMock.Object,
                                            ssMock,
                                            scsMock,
                                            loggerMock);

        // Act
        vm.OpenDocumentationCommand.Execute(anchor);

        // Assert
        fsaMock.Verify(m => m.OpenUrl(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void OpenDocumentation_Execute_StringAnchor()
    {
        // Arrange
        var project = new SqlProject("", "", "");
        var csMock = Mock.Of<IConfigurationService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ConfigurationViewModel(project,
                                            csMock,
                                            fsaMock.Object,
                                            ssMock,
                                            scsMock,
                                            loggerMock);

        // Act
        vm.OpenDocumentationCommand.Execute("test-help");

        // Assert
        fsaMock.Verify(m => m.OpenUrl("https://github.com/Herdo/SSDTLifecycleExtension/wiki/Configuration#test-help"), Times.Once);
    }

    [Test]
    public void ImportConfiguration_CanExecute_NotWhenScaffoldingIsInProgress()
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
            TrackDacpacVersion = false
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
        var canExecute = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canExecute.Should().BeFalse();
    }

    [Test]
    public void ImportConfiguration_CanExecute_YesAfterScaffoldingCompleted()
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
            TrackDacpacVersion = false
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
        var canExecuteDuringScaffolding = vm.ImportConfigurationCommand.CanExecute();
        ssMock.SetupGet(m => m.IsScaffolding).Returns(false);
        ssMock.Raise(service => service.IsScaffoldingChanged += null, EventArgs.Empty);
        var canExecuteAfterScaffolding = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canExecuteDuringScaffolding.Should().BeFalse();
        canExecuteAfterScaffolding.Should().BeTrue();
    }

    [Test]
    public void ImportConfiguration_CanExecute_NotWhenScriptCreationIsInProgress()
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
            TrackDacpacVersion = false
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
        var canExecute = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canExecute.Should().BeFalse();
    }

    [Test]
    public void ImportConfiguration_CanExecute_YesAfterScriptCreationCompleted()
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
            TrackDacpacVersion = false
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
        var canExecuteDuringScriptCreation = vm.ImportConfigurationCommand.CanExecute();
        scsMock.SetupGet(m => m.IsCreating).Returns(false);
        scsMock.Raise(service => service.IsCreatingChanged += null, EventArgs.Empty);
        var canExecuteAfterScriptCreation = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canExecuteDuringScriptCreation.Should().BeFalse();
        canExecuteAfterScriptCreation.Should().BeTrue();
    }

    [Test]
    public async Task ImportConfiguration_Execute_Async()
    {
        // Arrange
        const string testPath = "foobar";
        var oldModel = new ConfigurationModel();
        var newModel = new ConfigurationModel
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(testPath))
              .ReturnsAsync(newModel);
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.BrowseForFile(".json", "JSON (*.json)|*.json"))
               .Returns(testPath);
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ConfigurationViewModel(project,
                                            csMock.Object,
                                            fsaMock.Object,
                                            ssMock,
                                            scsMock,
                                            loggerMock)
        {
            Model = oldModel
        };

        // Act
        var canImportBeforeSaving = vm.ImportConfigurationCommand.CanExecute();
        await vm.ImportConfigurationCommand.ExecuteAsync();
        var canImportAfterSaving = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canImportBeforeSaving.Should().BeTrue();
        canImportAfterSaving.Should().BeTrue();
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(testPath), Times.Once);
        vm.Model.Should().BeSameAs(newModel);
    }

    [Test]
    public async Task ImportConfiguration_Execute_NoFileSelected_Async()
    {
        // Arrange
        var oldModel = new ConfigurationModel();
        var project = new SqlProject("", "", "");
        var csMock = new Mock<IConfigurationService>();
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.BrowseForFile(".json", "JSON (*.json)|*.json"))
               .Returns(null as string);
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ConfigurationViewModel(project,
                                            csMock.Object,
                                            fsaMock.Object,
                                            ssMock,
                                            scsMock,
                                            loggerMock)
        {
            Model = oldModel
        };

        // Act
        var canImportBeforeSaving = vm.ImportConfigurationCommand.CanExecute();
        await vm.ImportConfigurationCommand.ExecuteAsync();
        var canImportAfterSaving = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canImportBeforeSaving.Should().BeTrue();
        canImportAfterSaving.Should().BeTrue();
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(It.IsAny<string>()), Times.Never);
        vm.Model.Should().BeSameAs(oldModel);
    }

    [Test]
    public void ImportConfiguration_Execute_CallLoggerOnError()
    {
        // Arrange
        const string testPath = "foobar";
        var testException = new InvalidOperationException("test exception");
        var oldModel = new ConfigurationModel
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(testPath))
              .ThrowsAsync(testException);
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.BrowseForFile(".json", "JSON (*.json)|*.json"))
               .Returns(testPath);
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        Exception loggedException = null;
        string loggedMessage = null;
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()))
                  .Callback((Exception exception, string message) =>
                  {
                      loggedException = exception;
                      loggedMessage = message;
                  })
                  .Returns(Task.CompletedTask);
        var vm = new ConfigurationViewModel(project,
                                            csMock.Object,
                                            fsaMock.Object,
                                            ssMock,
                                            scsMock,
                                            loggerMock.Object)
        {
            Model = oldModel
        };

        // Act
        var canImportBeforeSaving = vm.ImportConfigurationCommand.CanExecute();
        vm.ImportConfigurationCommand.Execute(null);
        var canImportAfterSaving = vm.ImportConfigurationCommand.CanExecute();

        // Assert
        canImportBeforeSaving.Should().BeTrue();
        canImportAfterSaving.Should().BeTrue();
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(testPath), Times.Once);
        vm.Model.Should().BeSameAs(oldModel);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()), Times.Once);
        loggedException.Should().BeSameAs(testException);
        loggedMessage.Should().NotBeNull();
        loggedMessage.Should().Contain(nameof(ConfigurationViewModel.ImportConfigurationCommand));
    }

    [Test]
    public void ImportConfiguration_Execute_CallLoggerOnError_DoNotThrowExceptionFromLogger()
    {
        // Arrange
        const string testPath = "foobar";
        var oldModel = new ConfigurationModel
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
            TrackDacpacVersion = false
        };
        var project = new SqlProject("", "", "");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(testPath))
              .ThrowsAsync(new InvalidOperationException("test exception"));
        var fsaMock = new Mock<IFileSystemAccess>();
        fsaMock.Setup(m => m.BrowseForFile(".json", "JSON (*.json)|*.json"))
               .Returns(testPath);
        var ssMock = Mock.Of<IScaffoldingService>();
        var scsMock = Mock.Of<IScriptCreationService>();
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()))
                  .ThrowsAsync(new Exception("logger failed"));
        var vm = new ConfigurationViewModel(project,
                                            csMock.Object,
                                            fsaMock.Object,
                                            ssMock,
                                            scsMock,
                                            loggerMock.Object)
        {
            Model = oldModel
        };

        // Act
        Assert.DoesNotThrow(() => vm.ImportConfigurationCommand.Execute(null));

        // Assert
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(testPath), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()), Times.Once);
    }
}