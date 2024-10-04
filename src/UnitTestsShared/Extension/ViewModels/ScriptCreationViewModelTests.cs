namespace SSDTLifecycleExtension.UnitTests.Extension.ViewModels;

[TestFixture]
public class ScriptCreationViewModelTests
{
    private ConfigurationModel _configurationModel;
    private SqlProject _sqlProject;
    private Mock<IConfigurationService> _configurationService;
    private Mock<IScaffoldingService> _scaffoldingService;
    private Mock<IScriptCreationService> _scriptCreationService;
    private Mock<IArtifactsService> _artifactsService;
    private Mock<ILogger> _logger;
    private ScriptCreationViewModel _viewModel;

    [SetUp]
    public void Setup()
    {
        _configurationModel = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "{MAJOR}.{MINOR}",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        _sqlProject = new SqlProject("a", "b", "c");
        _configurationService = new();
        _configurationService.Setup(m => m.GetConfigurationOrDefaultAsync(_sqlProject))
            .ReturnsAsync(_configurationModel);
        _scaffoldingService = new();
        _scriptCreationService = new();
        _artifactsService = new();
        _artifactsService.Setup(m => m.GetExistingArtifactVersionsAsync(_sqlProject, _configurationModel))
            .ReturnsAsync(() =>
            [
                new VersionModel()
                {
                    IsNewestVersion = true,
                    UnderlyingVersion = new Version(4, 0)
                }
            ]);
        _logger = new();
        _viewModel = new(_sqlProject,
            _configurationService.Object,
            _scaffoldingService.Object,
            _scriptCreationService.Object,
            _artifactsService.Object,
            _logger.Object);
    }

    [Test]
    public void Constructor_CorrectInitialization()
    {
        // Assert
        _viewModel.ExistingVersions.Should().NotBeNull();
        _viewModel.SelectedBaseVersion.Should().BeNull();
        _viewModel.InitializedOnce.Should().BeFalse();
        _viewModel.ScaffoldingMode.Should().BeFalse();
        _viewModel.ExistingVersions.Should().BeEmpty();
        _viewModel.ScaffoldDevelopmentVersionCommand.Should().NotBeNull();
        _viewModel.ScaffoldCurrentProductionVersionCommand.Should().NotBeNull();
        _viewModel.StartLatestCreationCommand.Should().NotBeNull();
        _viewModel.StartVersionedCreationCommand.Should().NotBeNull();
    }

    [Test]
    public void SelectedBaseVersion_NoPropertyChangedForSameInstance()
    {
        // Arrange
        var version = new VersionModel();
        _viewModel.SelectedBaseVersion = version;
        object invokedSender = null;
        string invokedProperty = null;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            invokedSender = sender;
            invokedProperty = args?.PropertyName;
        };

        // Act
        _viewModel.SelectedBaseVersion = version;

        // Assert
        invokedSender.Should().BeNull();
        invokedProperty.Should().BeNull();
    }

    [Test]
    public void SelectedBaseVersion_Get_Set_PropertyChanged()
    {
        // Arrange
        var version = new VersionModel();
        object invokedSender = null;
        string invokedProperty = null;
        _viewModel.PropertyChanged += (sender, args) =>
        {
            invokedSender = sender;
            invokedProperty = args?.PropertyName;
        };

        // Act
        _viewModel.SelectedBaseVersion = version;
        var setVersion = _viewModel.SelectedBaseVersion;

        // Assert
        invokedSender.Should().BeSameAs(_viewModel);
        setVersion.Should().BeSameAs(version);
        invokedProperty.Should().Be(nameof(ScriptCreationViewModel.SelectedBaseVersion));
    }

    [Test]
    public void ArtifactCommands_CanExecute_NotWhenNotInitialized()
    {
        // Act
        var canExecuteList = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };

        // Assert
        canExecuteList.Should().AllBeEquivalentTo(false);
    }

    [Test]
    public async Task ArtifactCommands_CanExecute_NotWhenConfigurationHasErrors_Async()
    {
        // Arrange
        _configurationModel.VersionPattern = "foobar";
        await _viewModel.InitializeAsync();

        // Act
        var canExecuteList = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };

        // Assert
        canExecuteList.Should().AllBeEquivalentTo(false);
    }

    [Test]
    public async Task ArtifactCommands_CanExecute_NotWhenScaffoldingIsInProgress_Async()
    {
        // Arrange
        _scaffoldingService.SetupGet(m => m.IsScaffolding)
            .Returns(true);
        await _viewModel.InitializeAsync();

        // Act
        var canExecuteList = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };

        // Assert
        canExecuteList.Should().AllBeEquivalentTo(false);
    }

    [Test]
    public async Task ArtifactCommands_CanExecute_AfterScaffoldingCompleted_Async()
    {
        // Arrange
        _scaffoldingService.SetupGet(m => m.IsScaffolding)
            .Returns(true);
        await _viewModel.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChanged = false;
        _viewModel.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender, args) => scaffoldDevelopmentVersionCommandCanExecuteChanged = true;
        var scaffoldCurrentProductionVersionCommandCanExecuteChanged = false;
        _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender, args) => scaffoldCurrentProductionVersionCommandCanExecuteChanged = true;
        var startLatestCreationCommandCanExecuteChanged = false;
        _viewModel.StartLatestCreationCommand.CanExecuteChanged += (sender, args) => startLatestCreationCommandCanExecuteChanged = true;
        var startVersionedCreationCommandCanExecuteChanged = false;
        _viewModel.StartVersionedCreationCommand.CanExecuteChanged += (sender, args) => startVersionedCreationCommandCanExecuteChanged = true;

        // Act
        var canExecuteList = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };
        _scaffoldingService.SetupGet(m => m.IsScaffolding)
            .Returns(false);
        _scaffoldingService.Raise(service => service.IsScaffoldingChanged += null, EventArgs.Empty);
        var canExecuteListAfterCompletion = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };

        // Assert
        canExecuteList.Should().AllBeEquivalentTo(false);
        canExecuteListAfterCompletion.Should().AllBeEquivalentTo(true);
        scaffoldDevelopmentVersionCommandCanExecuteChanged.Should().BeTrue();
        scaffoldCurrentProductionVersionCommandCanExecuteChanged.Should().BeTrue();
        startLatestCreationCommandCanExecuteChanged.Should().BeTrue();
        startVersionedCreationCommandCanExecuteChanged.Should().BeTrue();
    }

    [Test]
    public async Task ArtifactCommands_CanExecute_NotWhenScriptCreationIsInProgress_Async()
    {
        // Arrange
        _scriptCreationService.SetupGet(m => m.IsCreating)
            .Returns(true);
        await _viewModel.InitializeAsync();

        // Act
        var canExecuteList = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };

        // Assert
        canExecuteList.Should().AllBeEquivalentTo(false);
    }

    [Test]
    public async Task ArtifactCommands_CanExecute_AfterScriptCreationCompleted_Async()
    {
        // Arrange
        _scriptCreationService.SetupGet(m => m.IsCreating)
            .Returns(true);
        await _viewModel.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChanged = false;
        _viewModel.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender, args) => scaffoldDevelopmentVersionCommandCanExecuteChanged = true;
        var scaffoldCurrentProductionVersionCommandCanExecuteChanged = false;
        _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender, args) => scaffoldCurrentProductionVersionCommandCanExecuteChanged = true;
        var startLatestCreationCommandCanExecuteChanged = false;
        _viewModel.StartLatestCreationCommand.CanExecuteChanged += (sender, args) => startLatestCreationCommandCanExecuteChanged = true;
        var startVersionedCreationCommandCanExecuteChanged = false;
        _viewModel.StartVersionedCreationCommand.CanExecuteChanged += (sender, args) => startVersionedCreationCommandCanExecuteChanged = true;

        // Act
        var canExecuteList = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };
        _scriptCreationService.SetupGet(m => m.IsCreating)
            .Returns(false);
        _scriptCreationService.Raise(service => service.IsCreatingChanged += null, EventArgs.Empty);
        var canExecuteListAfterCompletion = new[]
        {
            _viewModel.ScaffoldDevelopmentVersionCommand.CanExecute(),
            _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecute(),
            _viewModel.StartLatestCreationCommand.CanExecute(),
            _viewModel.StartVersionedCreationCommand.CanExecute()
        };

        // Assert
        canExecuteList.Should().AllBeEquivalentTo(false);
        canExecuteListAfterCompletion.Should().AllBeEquivalentTo(true);
        scaffoldDevelopmentVersionCommandCanExecuteChanged.Should().BeTrue();
        scaffoldCurrentProductionVersionCommandCanExecuteChanged.Should().BeTrue();
        startLatestCreationCommandCanExecuteChanged.Should().BeTrue();
        startVersionedCreationCommandCanExecuteChanged.Should().BeTrue();
    }

    [Test]
    public async Task InitializeAsync_NoExistingVersions_Async()
    {
        // Arrange
        _artifactsService.Setup(m => m.GetExistingArtifactVersionsAsync(_sqlProject, _configurationModel))
            .ReturnsAsync([]);

        // Act
        var initialized = await _viewModel.InitializeAsync();

        // Assert
        initialized.Should().BeTrue();
        _viewModel.InitializedOnce.Should().BeTrue();
        _viewModel.ExistingVersions.Should().BeEmpty();
        _viewModel.SelectedBaseVersion.Should().BeNull();
        _viewModel.ScaffoldingMode.Should().BeTrue();
    }

    [Test]
    public async Task InitializeAsync_ValidDirectories_SelectHighest_Async()
    {
        // Arrange
        _artifactsService.Setup(m => m.GetExistingArtifactVersionsAsync(_sqlProject, _configurationModel))
            .ReturnsAsync(() =>
            [
                new VersionModel
                {
                    IsNewestVersion = true,
                    UnderlyingVersion = new Version(5, 0)
                },
                new VersionModel
                {
                    UnderlyingVersion = new Version(4, 0, 0)
                }
            ]);

        // Act
        var initialized = await _viewModel.InitializeAsync();

        // Assert
        initialized.Should().BeTrue();
        _viewModel.InitializedOnce.Should().BeTrue();
        _viewModel.ExistingVersions.Should().HaveCount(2);
        _viewModel.SelectedBaseVersion.Should().NotBeNull();
        _viewModel.SelectedBaseVersion.Should().BeSameAs(_viewModel.ExistingVersions[0]);
        _viewModel.ExistingVersions[0].IsNewestVersion.Should().BeTrue();
        _viewModel.ExistingVersions[0].UnderlyingVersion.Should().Be(new Version(5, 0));
        _viewModel.ExistingVersions[1].IsNewestVersion.Should().BeFalse();
        _viewModel.ExistingVersions[1].UnderlyingVersion.Should().Be(new Version(4, 0, 0));
        _viewModel.ScaffoldingMode.Should().BeFalse();
    }

    [Test]
    public async Task ConfigurationService_ConfigurationChanged_SameProject_Async()
    {
        // Arrange
        await _viewModel.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        _viewModel.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender, args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender, args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        _viewModel.StartLatestCreationCommand.CanExecuteChanged += (sender, args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        _viewModel.StartVersionedCreationCommand.CanExecuteChanged += (sender, args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        _configurationService.Raise(m => m.ConfigurationChanged += null, new ProjectConfigurationChangedEventArgs(_sqlProject));

        // Assert
        _configurationService.Verify(m => m.GetConfigurationOrDefaultAsync(_sqlProject), Times.Exactly(2));
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(1);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(1);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(1);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(1);
    }

    [Test]
    public async Task ConfigurationService_ConfigurationChanged_DifferentProject_Async()
    {
        // Arrange
        var differentProject = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "d");
        await _viewModel.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        _viewModel.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        _viewModel.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        _viewModel.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        _configurationService.Raise(m => m.ConfigurationChanged += null, new ProjectConfigurationChangedEventArgs(differentProject));

        // Assert
        _configurationService.Verify(m => m.GetConfigurationOrDefaultAsync(_sqlProject), Times.Exactly(1));
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(0);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(0);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(0);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(0);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task ScaffoldDevelopmentVersionCommand_ExecutedAsync_Async(bool success)
    {
        _scaffoldingService.Setup(m => m.ScaffoldAsync(_sqlProject, _configurationModel, new Version(0, 0, 0, 0), CancellationToken.None))
            .ReturnsAsync(success);
        await _viewModel.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        _viewModel.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        _viewModel.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        _viewModel.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        _viewModel.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        await _viewModel.ScaffoldDevelopmentVersionCommand.ExecuteAsync();

        // Assert
        var expectedConfigurationLoads = success ? 2 : 1;
        _configurationService.Verify(m => m.GetConfigurationOrDefaultAsync(_sqlProject), Times.Exactly(expectedConfigurationLoads));
        _scaffoldingService.Verify(m => m.ScaffoldAsync(_sqlProject, _configurationModel, new Version(0, 0, 0, 0), CancellationToken.None), Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(3);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(1);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(1);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(1);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task ScaffoldCurrentProductionVersionCommand_ExecutedAsync_Async(bool success)
    {
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        ssMock.Setup(m => m.ScaffoldAsync(project, config, new Version(1, 0, 0, 0), CancellationToken.None))
              .ReturnsAsync(success);
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(() =>
              [
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              ]);
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock);
        await vm.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        await vm.ScaffoldCurrentProductionVersionCommand.ExecuteAsync();

        // Assert
        var expectedConfigurationLoads = success ? 2 : 1;
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Exactly(expectedConfigurationLoads));
        ssMock.Verify(m => m.ScaffoldAsync(project, config, new Version(1, 0, 0, 0), CancellationToken.None), Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(1);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(3);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(1);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(1);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task StartLatestCreationCommand_ExecutedAsync_Async(bool success)
    {
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(() =>
              [
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              ]);
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock);
        bool? isCreatingScriptDuringCall = null;
        scsMock.Setup(m => m.CreateAsync(project, config, new Version(4, 0), true, CancellationToken.None))
               .Callback(() => isCreatingScriptDuringCall = true)
               .ReturnsAsync(success);
        var invokedIsCreatingScriptCount = 0;
        vm.PropertyChanged += (sender,
                               args) =>
        {
            if (ReferenceEquals(sender, vm) && args?.PropertyName == nameof(ScriptCreationViewModel.IsCreatingScript))
                invokedIsCreatingScriptCount++;
        };
        await vm.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        await vm.StartLatestCreationCommand.ExecuteAsync();
        var isCreatingScriptAfterCall = vm.IsCreatingScript;

        // Assert
        var expectedConfigurationLoads = success ? 2 : 1;
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Exactly(expectedConfigurationLoads));
        scsMock.Verify(m => m.CreateAsync(project, config, new Version(4, 0), true, CancellationToken.None), Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(1);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(1);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(3);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(1);
        isCreatingScriptDuringCall.Should().BeTrue();
        isCreatingScriptAfterCall.Should().BeFalse();
        invokedIsCreatingScriptCount.Should().Be(2);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public async Task StartVersionedCreationCommand_ExecutedAsync_Async(bool success)
    {
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(() =>
              [
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              ]);
        var loggerMock = Mock.Of<ILogger>();
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock);
        bool? isCreatingScriptDuringCall = null;
        scsMock.Setup(m => m.CreateAsync(project, config, new Version(4, 0), false, CancellationToken.None))
               .Callback(() => isCreatingScriptDuringCall = true)
               .ReturnsAsync(success);
        var invokedIsCreatingScriptCount = 0;
        vm.PropertyChanged += (sender,
                               args) =>
        {
            if (ReferenceEquals(sender, vm) && args?.PropertyName == nameof(ScriptCreationViewModel.IsCreatingScript))
                invokedIsCreatingScriptCount++;
        };
        await vm.InitializeAsync();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        await vm.StartVersionedCreationCommand.ExecuteAsync();
        var isCreatingScriptAfterCall = vm.IsCreatingScript;

        // Assert
        var expectedConfigurationLoads = success ? 2 : 1;
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Exactly(expectedConfigurationLoads));
        scsMock.Verify(m => m.CreateAsync(project, config, new Version(4, 0), false, CancellationToken.None), Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(1);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(1);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(1);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(3);
        isCreatingScriptDuringCall.Should().BeTrue();
        isCreatingScriptAfterCall.Should().BeFalse();
        invokedIsCreatingScriptCount.Should().Be(2);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ScaffoldDevelopmentVersionCommand_Executed_CallLoggerOnError(bool loggerThrowsException)
    {
        var testException = new InvalidOperationException("test exception");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        ssMock.Setup(m => m.ScaffoldAsync(project, config, new Version(0, 0, 0, 0), CancellationToken.None))
              .ThrowsAsync(testException);
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(new[]
              {
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              });
        var loggerMock = new Mock<ILogger>();
        if (loggerThrowsException)
        {
            loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()))
                      .ThrowsAsync(new InvalidOperationException("logger exception"));
        }
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock.Object);
        var initialized = vm.InitializeAsync().Result;
        initialized.Should().BeTrue();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        Assert.DoesNotThrow(() => vm.ScaffoldDevelopmentVersionCommand.Execute(null));

        // Assert
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Once);
        ssMock.Verify(m => m.ScaffoldAsync(project, config, new Version(0, 0, 0, 0), CancellationToken.None), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(testException, It.Is<string>(message => message.Contains(nameof(ScriptCreationViewModel.ScaffoldDevelopmentVersionCommand)))),
                          Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(1);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(0);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(0);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(0);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ScaffoldCurrentProductionVersionCommand_Executed_CallLoggerOnError(bool loggerThrowsException)
    {
        var testException = new InvalidOperationException("test exception");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        ssMock.Setup(m => m.ScaffoldAsync(project, config, new Version(1, 0, 0, 0), CancellationToken.None))
              .ThrowsAsync(testException);
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(new[]
              {
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              });
        var loggerMock = new Mock<ILogger>();
        if (loggerThrowsException)
        {
            loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()))
                      .ThrowsAsync(new InvalidOperationException("logger exception"));
        }
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock.Object);
        var initialized = vm.InitializeAsync().Result;
        initialized.Should().BeTrue();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        Assert.DoesNotThrow(() => vm.ScaffoldCurrentProductionVersionCommand.Execute(null));

        // Assert
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Once);
        ssMock.Verify(m => m.ScaffoldAsync(project, config, new Version(1, 0, 0, 0), CancellationToken.None), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(testException, It.Is<string>(message => message.Contains(nameof(ScriptCreationViewModel.ScaffoldCurrentProductionVersionCommand)))),
                          Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(0);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(1);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(0);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(0);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void StartLatestCreationCommand_Executed_CallLoggerOnError(bool loggerThrowsException)
    {
        var testException = new InvalidOperationException("test exception");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(new[]
              {
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              });
        var loggerMock = new Mock<ILogger>();
        if (loggerThrowsException)
        {
            loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()))
                      .ThrowsAsync(new InvalidOperationException("logger exception"));
        }
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock.Object);
        bool? isCreatingScriptDuringCall = null;
        scsMock.Setup(m => m.CreateAsync(project, config, new Version(4, 0), true, CancellationToken.None))
               .Callback(() => isCreatingScriptDuringCall = true)
               .ThrowsAsync(testException);
        var invokedIsCreatingScriptCount = 0;
        vm.PropertyChanged += (sender,
                               args) =>
        {
            if (ReferenceEquals(sender, vm) && args?.PropertyName == nameof(ScriptCreationViewModel.IsCreatingScript))
                invokedIsCreatingScriptCount++;
        };
        var initialized = vm.InitializeAsync().Result;
        initialized.Should().BeTrue();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        Assert.DoesNotThrow(() => vm.StartLatestCreationCommand.Execute(null));
        var isCreatingScriptAfterCall = vm.IsCreatingScript;

        // Assert
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Once);
        scsMock.Verify(m => m.CreateAsync(project, config, new Version(4, 0), true, CancellationToken.None), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(testException, It.Is<string>(message => message.Contains(nameof(ScriptCreationViewModel.StartLatestCreationCommand)))),
                          Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(0);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(0);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(1);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(0);
        isCreatingScriptDuringCall.Should().BeTrue();
        isCreatingScriptAfterCall.Should().BeFalse();
        invokedIsCreatingScriptCount.Should().Be(2);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void StartVersionedCreationCommand_Executed_CallLoggerOnError(bool loggerThrowsException)
    {
        var testException = new InvalidOperationException("test exception");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "_Deployment",
            PublishProfilePath = "TestProfile.publish.xml",
            ReplaceUnnamedDefaultConstraintDrops = false,
            VersionPattern = "1.2.3.4",
            CommentOutUnnamedDefaultConstraintDrops = true,
            CreateDocumentationWithScriptCreation = false,
            CustomHeader = "TestHeader",
            CustomFooter = "TestFooter",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = true
        };
        var project = new SqlProject("a", @"C:\TestProject\TestProject.sqlproj", "c");
        var csMock = new Mock<IConfigurationService>();
        csMock.Setup(m => m.GetConfigurationOrDefaultAsync(project))
              .ReturnsAsync(config);
        var ssMock = new Mock<IScaffoldingService>();
        var scsMock = new Mock<IScriptCreationService>();
        var asMock = new Mock<IArtifactsService>();
        asMock.Setup(m => m.GetExistingArtifactVersionsAsync(project, config))
              .ReturnsAsync(new[]
              {
                  new VersionModel
                  {
                      IsNewestVersion = true,
                      UnderlyingVersion = new Version(4, 0)
                  }
              });
        var loggerMock = new Mock<ILogger>();
        if (loggerThrowsException)
        {
            loggerMock.Setup(m => m.LogErrorAsync(It.IsAny<Exception>(), It.IsAny<string>()))
                      .ThrowsAsync(new InvalidOperationException("logger exception"));
        }
        var vm = new ScriptCreationViewModel(project, csMock.Object, ssMock.Object, scsMock.Object, asMock.Object, loggerMock.Object);
        bool? isCreatingScriptDuringCall = null;
        scsMock.Setup(m => m.CreateAsync(project, config, new Version(4, 0), false, CancellationToken.None))
               .Callback(() => isCreatingScriptDuringCall = true)
               .ThrowsAsync(testException);
        var invokedIsCreatingScriptCount = 0;
        vm.PropertyChanged += (sender,
                               args) =>
        {
            if (ReferenceEquals(sender, vm) && args?.PropertyName == nameof(ScriptCreationViewModel.IsCreatingScript))
                invokedIsCreatingScriptCount++;
        };
        var initialized = vm.InitializeAsync().Result;
        initialized.Should().BeTrue();
        var scaffoldDevelopmentVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldDevelopmentVersionCommand.CanExecuteChanged += (sender,
                                                                   args) => scaffoldDevelopmentVersionCommandCanExecuteChangedCount++;
        var scaffoldCurrentProductionVersionCommandCanExecuteChangedCount = 0;
        vm.ScaffoldCurrentProductionVersionCommand.CanExecuteChanged += (sender,
                                                                         args) => scaffoldCurrentProductionVersionCommandCanExecuteChangedCount++;
        var startLatestCreationCommandCanExecuteChangedCount = 0;
        vm.StartLatestCreationCommand.CanExecuteChanged += (sender,
                                                            args) => startLatestCreationCommandCanExecuteChangedCount++;
        var startVersionedCreationCommandCanExecuteChangedCount = 0;
        vm.StartVersionedCreationCommand.CanExecuteChanged += (sender,
                                                               args) => startVersionedCreationCommandCanExecuteChangedCount++;

        // Act
        Assert.DoesNotThrow(() => vm.StartVersionedCreationCommand.Execute(null));
        var isCreatingScriptAfterCall = vm.IsCreatingScript;

        // Assert
        csMock.Verify(m => m.GetConfigurationOrDefaultAsync(project), Times.Once);
        scsMock.Verify(m => m.CreateAsync(project, config, new Version(4, 0), false, CancellationToken.None), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync(testException, It.Is<string>(message => message.Contains(nameof(ScriptCreationViewModel.StartVersionedCreationCommand)))),
                          Times.Once);
        scaffoldDevelopmentVersionCommandCanExecuteChangedCount.Should().Be(0);
        scaffoldCurrentProductionVersionCommandCanExecuteChangedCount.Should().Be(0);
        startLatestCreationCommandCanExecuteChangedCount.Should().Be(0);
        startVersionedCreationCommandCanExecuteChangedCount.Should().Be(1);
        isCreatingScriptDuringCall.Should().BeTrue();
        isCreatingScriptAfterCall.Should().BeFalse();
        invokedIsCreatingScriptCount.Should().Be(2);
    }
}