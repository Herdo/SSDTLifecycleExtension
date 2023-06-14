namespace SSDTLifecycleExtension.ViewModels;

[UsedImplicitly]
public class ConfigurationViewModel : ViewModelBase,
    IErrorHandler
{
    private readonly SqlProject _project;
    private readonly IConfigurationService _configurationService;
    private readonly IFileSystemAccess _fileSystemAccess;
    private readonly IScaffoldingService _scaffoldingService;
    private readonly IScriptCreationService _scriptCreationService;
    private readonly ILogger _logger;

    private ConfigurationModel _lastSavedModel;
    private ConfigurationModel _model;
    private bool _isModelDirty;

    public ConfigurationModel Model
    {
        get => _model;
        set
        {
            if (Equals(value, _model)) return;
            if (_model != null)
            {
                _model.PropertyChanged -= Model_PropertyChanged;
                _model.ErrorsChanged -= Model_ErrorsChanged;
            }
            _model = value;
            if (_model != null)
            {
                _model.PropertyChanged += Model_PropertyChanged;
                _model.ErrorsChanged += Model_ErrorsChanged;
            }
            OnPropertyChanged();
            CheckIfModelIsDirty();
        }
    }

    public bool IsModelDirty
    {
        get => _isModelDirty;
        private set
        {
            if (value == _isModelDirty) return;
            _isModelDirty = value;
            OnPropertyChanged();
            SaveConfigurationCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand BrowsePublishProfileCommand { get; }
    public ICommand ResetConfigurationToDefaultCommand { get; }
    public IAsyncCommand SaveConfigurationCommand { get; }
    public ICommand OpenDocumentationCommand { get; }
    public IAsyncCommand ImportConfigurationCommand { get; }

    public ConfigurationViewModel([NotNull] SqlProject project,
                                  [NotNull] IConfigurationService configurationService,
                                  [NotNull] IFileSystemAccess fileSystemAccess,
                                  [NotNull] IScaffoldingService scaffoldingService,
                                  [NotNull] IScriptCreationService scriptCreationService,
                                  [NotNull] ILogger logger)
    {
        _project = project ?? throw new ArgumentNullException(nameof(project));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        _fileSystemAccess = fileSystemAccess ?? throw new ArgumentNullException(nameof(fileSystemAccess));
        _scaffoldingService = scaffoldingService ?? throw new ArgumentNullException(nameof(scaffoldingService));
        _scriptCreationService = scriptCreationService ?? throw new ArgumentNullException(nameof(scriptCreationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scaffoldingService.IsScaffoldingChanged += ScaffoldingService_IsScaffoldingChanged;
        _scriptCreationService.IsCreatingChanged += ScriptCreationService_IsCreatingChanged;

        BrowsePublishProfileCommand = new DelegateCommand(BrowsePublishProfile_Executed, BrowsePublishProfile_CanExecute);
        ResetConfigurationToDefaultCommand = new DelegateCommand(ResetConfigurationToDefault_Executed);
        SaveConfigurationCommand = new AsyncCommand(SaveConfiguration_ExecutedAsync, SaveConfiguration_CanExecute, this);
        OpenDocumentationCommand = new DelegateCommand(OpenDocumentation_Executed);
        ImportConfigurationCommand = new AsyncCommand(ImportConfiguration_ExecutedAsync, ImportConfiguration_CanExecute, this);
    }

    private bool BrowsePublishProfile_CanExecute() => Model != null;

    private void BrowsePublishProfile_Executed()
    {
        var browsedPath = _fileSystemAccess.BrowseForFile(".publish.xml", "Publish Profile (*.publish.xml)|*.publish.xml");
        if (browsedPath == null)
            return;

        var projectPath = new Uri(_project.FullName, UriKind.Absolute);
        var profilePath = new Uri(browsedPath, UriKind.Absolute);
        var relativePath = projectPath.MakeRelativeUri(profilePath).ToString();
        Model.PublishProfilePath = relativePath;
    }

    private void ResetConfigurationToDefault_Executed()
    {
        Model = ConfigurationModel.GetDefault();
        Model.ValidateAll();
    }

    private bool SaveConfiguration_CanExecute()
    {
        return Model != null
            && !Model.HasErrors
            && IsModelDirty
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;
    }

    private async Task SaveConfiguration_ExecutedAsync()
    {
        var copy = Model.Copy();
        var saved = await _configurationService.SaveConfigurationAsync(_project, copy);
        if (!saved)
            return;

        _lastSavedModel = copy;
        CheckIfModelIsDirty();
    }

    private void OpenDocumentation_Executed(object param)
    {
        if (!(param is string anchor))
            return;
        _fileSystemAccess.OpenUrl(Constants.DocumentationBaseUrl + anchor);
    }

    private bool ImportConfiguration_CanExecute()
    {
        return !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;
    }

    private async Task ImportConfiguration_ExecutedAsync()
    {
        var browsedPath = _fileSystemAccess.BrowseForFile(".json", "JSON (*.json)|*.json");
        if (browsedPath == null)
            return;

        Model = await _configurationService.GetConfigurationOrDefaultAsync(browsedPath);
    }

    private void CheckIfModelIsDirty()
    {
        if (Model == null)
        {
            IsModelDirty = false;
            return;
        }

        if (Model != null && _lastSavedModel == null)
        {
            IsModelDirty = true;
            return;
        }

        // Check by properties
        IsModelDirty = !Model.Equals(_lastSavedModel);
    }

    private void UpdateAsyncCommandsCanExecuteState()
    {
        SaveConfigurationCommand.RaiseCanExecuteChanged();
        ImportConfigurationCommand.RaiseCanExecuteChanged();
    }

    public override async Task<bool> InitializeAsync()
    {
        _lastSavedModel = await _configurationService.GetConfigurationOrDefaultAsync(_project);
        Model = _lastSavedModel.Copy();
        return true;
    }

    private void ScaffoldingService_IsScaffoldingChanged(object sender, EventArgs e) => UpdateAsyncCommandsCanExecuteState();

    private void ScriptCreationService_IsCreatingChanged(object sender, EventArgs e) => UpdateAsyncCommandsCanExecuteState();

    private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        CheckIfModelIsDirty();
    }

    private void Model_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
    {
        SaveConfigurationCommand.RaiseCanExecuteChanged();
    }

    async Task IErrorHandler.HandleErrorAsync(IAsyncCommand command, Exception exception)
    {
        var commandName = string.Empty;
        if (ReferenceEquals(SaveConfigurationCommand, command))
        {
            commandName = nameof(SaveConfigurationCommand);
        }
        else if (ReferenceEquals(ImportConfigurationCommand, command))
        {
            commandName = nameof(ImportConfigurationCommand);
        }

        try
        {
            await _logger.LogErrorAsync(exception, $"Error during execution of {commandName}").ConfigureAwait(false);
        }
        catch
        {
            // ignored - when logging the exception fails, we don't want to end up in a stack overflow.
        }
    }
}