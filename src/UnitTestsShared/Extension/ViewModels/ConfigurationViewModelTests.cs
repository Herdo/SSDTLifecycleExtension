namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels;

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
        Assert.IsNotNull(vm.OpenDocumentationCommand);
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

        var changedSenders = new List<object>();
        var changedProperties = new List<string>();
        var errorChangedSenderList = new List<object>();
        var errorChangedPropertyList = new List<string>();
        ConfigurationModel propertyChangedModel = null;
        vm.PropertyChanged += (sender,
                               args) =>
        {
            if (sender != null)
                changedSenders.Add(sender);
            if (args?.PropertyName != null)
                changedProperties.Add(args.PropertyName);
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
        Assert.AreEqual(2, changedSenders.Count);
        Assert.AreSame(vm, changedSenders[0]);
        Assert.AreSame(vm, changedSenders[1]);
        Assert.AreEqual(2, changedProperties.Count);
        Assert.AreEqual(nameof(ConfigurationViewModel.Model), changedProperties[0]);
        Assert.AreEqual(nameof(ConfigurationViewModel.IsModelDirty), changedProperties[1]);
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
                                            loggerMock)
        {
            Model = ConfigurationModel.GetDefault()
        };
        vm.Model = null;

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
        Assert.IsFalse(canExecuteDuringScriptCreation);
        Assert.IsTrue(canExecuteAfterScriptCreation);
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
        Assert.IsTrue(canSaveBeforeSaving);
        Assert.IsTrue(canSaveAfterSaving);
        csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
        Assert.IsNotNull(savedModel);
        Assert.AreNotSame(model, savedModel);
        Assert.AreEqual(model, savedModel);
        Assert.AreSame(model, vm.Model);
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
              .Callback((SqlProject p,
                         ConfigurationModel m) => savedModel = m)
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
        Assert.IsTrue(canSaveBeforeSaving);
        Assert.IsTrue(canSaveAfterSaving);
        csMock.Verify(m => m.SaveConfigurationAsync(project, It.IsNotNull<ConfigurationModel>()), Times.Once());
        Assert.IsNotNull(savedModel);
        Assert.AreNotSame(model, savedModel);
        Assert.AreEqual(model, savedModel);
        Assert.AreSame(model, vm.Model);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()), Times.Once);
        Assert.AreSame(testException, loggedException);
        Assert.IsNotNull(loggedMessage);
        Assert.IsTrue(loggedMessage.Contains(nameof(ConfigurationViewModel.SaveConfigurationCommand)));
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
        Assert.IsTrue(canExecute);
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
        Assert.IsFalse(canExecute);
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
        Assert.IsFalse(canExecuteDuringScaffolding);
        Assert.IsTrue(canExecuteAfterScaffolding);
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
        Assert.IsFalse(canExecute);
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
        Assert.IsFalse(canExecuteDuringScriptCreation);
        Assert.IsTrue(canExecuteAfterScriptCreation);
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
        Assert.IsTrue(canImportBeforeSaving);
        Assert.IsTrue(canImportAfterSaving);
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(testPath), Times.Once);
        Assert.AreSame(newModel, vm.Model);
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
        Assert.IsTrue(canImportBeforeSaving);
        Assert.IsTrue(canImportAfterSaving);
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(It.IsAny<string>()), Times.Never);
        Assert.AreSame(oldModel, vm.Model);
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
        Assert.IsTrue(canImportBeforeSaving);
        Assert.IsTrue(canImportAfterSaving);
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(testPath), Times.Once);
        Assert.AreSame(oldModel, vm.Model);
        loggerMock.Verify(m => m.LogErrorAsync(It.IsNotNull<Exception>(), It.IsNotNull<string>()), Times.Once);
        Assert.AreSame(testException, loggedException);
        Assert.IsNotNull(loggedMessage);
        Assert.IsTrue(loggedMessage.Contains(nameof(ConfigurationViewModel.ImportConfigurationCommand)));
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