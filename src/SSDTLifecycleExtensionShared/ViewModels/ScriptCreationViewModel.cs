﻿#nullable enable

namespace SSDTLifecycleExtension.ViewModels;

public class ScriptCreationViewModel : ViewModelBase,
    IErrorHandler
{
    private readonly SqlProject _project;
    private readonly IConfigurationService _configurationService;
    private readonly IScaffoldingService _scaffoldingService;
    private readonly IScriptCreationService _scriptCreationService;
    private readonly IArtifactsService _artifactsService;
    private readonly ILogger _logger;

    private ConfigurationModel? _configuration;

    private VersionModel? _selectedBaseVersion;
    private bool _scaffoldingMode;
    private bool _isCreatingScript;
    private bool _initializedOnce;

    public VersionModel? SelectedBaseVersion
    {
        get => _selectedBaseVersion;
        set
        {
            if (Equals(value, _selectedBaseVersion))
                return;
            _selectedBaseVersion = value;
            OnPropertyChanged();
            EvaluateCommands();
        }
    }

    public bool InitializedOnce
    {
        get => _initializedOnce;
        private set
        {
            if (value == _initializedOnce)
                return;
            _initializedOnce = value;
            OnPropertyChanged();
        }
    }

    public bool ScaffoldingMode
    {
        get => _scaffoldingMode;
        private set
        {
            if (value == _scaffoldingMode)
                return;
            _scaffoldingMode = value;
            OnPropertyChanged();
        }
    }

    public bool IsCreatingScript
    {
        get => _isCreatingScript;
        private set
        {
            _isCreatingScript = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<VersionModel> ExistingVersions { get; }

    public IAsyncCommand ScaffoldDevelopmentVersionCommand { get; }

    public IAsyncCommand ScaffoldCurrentProductionVersionCommand { get; }

    public IAsyncCommand StartLatestCreationCommand { get; }

    public IAsyncCommand StartVersionedCreationCommand { get; }

    public ScriptCreationViewModel(SqlProject project,
        IConfigurationService configurationService,
        IScaffoldingService scaffoldingService,
        IScriptCreationService scriptCreationService,
        IArtifactsService artifactsService,
        ILogger logger)
    {
        _project = project;
        _configurationService = configurationService;
        _scaffoldingService = scaffoldingService;
        _scriptCreationService = scriptCreationService;
        _artifactsService = artifactsService;
        _logger = logger;

        ExistingVersions = new ObservableCollection<VersionModel>();

        ScaffoldDevelopmentVersionCommand = new AsyncCommand(ScaffoldDevelopmentVersion_ExecutedAsync, ScaffoldCommands_CanExecute, this);
        ScaffoldCurrentProductionVersionCommand = new AsyncCommand(ScaffoldCurrentProductionVersion_ExecutedAsync, ScaffoldCommands_CanExecute, this);
        StartLatestCreationCommand = new AsyncCommand(StartLatestCreation_ExecutedAsync, ScriptCommands_CanExecute, this);
        StartVersionedCreationCommand = new AsyncCommand(StartVersionedCreation_ExecutedAsync, ScriptCommands_CanExecute, this);

        _configurationService.ConfigurationChanged += ConfigurationService_ConfigurationChanged;
        _scaffoldingService.IsScaffoldingChanged += ScaffoldingService_IsScaffoldingChanged;
        _scriptCreationService.IsCreatingChanged += ScriptCreationService_IsCreatingChanged;
    }

    private async Task ScaffoldInternalAsync(Version targetVersion)
    {
        var successful = await _scaffoldingService.ScaffoldAsync(_project, _configuration!, targetVersion, CancellationToken.None);
        if (successful)
            await InitializeAsync();
        else
            EvaluateCommands();
    }

    private async Task CreateScriptInternalAsync(bool latest)
    {
        IsCreatingScript = true;
        try
        {
            var successful = await _scriptCreationService.CreateAsync(_project, _configuration!, SelectedBaseVersion!.UnderlyingVersion, latest, CancellationToken.None);
            if (successful)
                await InitializeAsync();
            else
                EvaluateCommands();
        }
        finally
        {
            IsCreatingScript = false;
        }
    }

    private bool ScaffoldCommands_CanExecute()
    {
        return _configuration is not null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;
    }

    private bool ScriptCommands_CanExecute()
    {
        return _configuration is not null
            && SelectedBaseVersion is not null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;
    }

    private async Task ScaffoldDevelopmentVersion_ExecutedAsync()
    {
        await ScaffoldInternalAsync(new Version(0, 0, 0, 0));
    }

    private async Task ScaffoldCurrentProductionVersion_ExecutedAsync()
    {
        await ScaffoldInternalAsync(new Version(1, 0, 0, 0));
    }

    private async Task StartLatestCreation_ExecutedAsync()
    {
        await CreateScriptInternalAsync(true);
    }

    private async Task StartVersionedCreation_ExecutedAsync()
    {
        await CreateScriptInternalAsync(false);
    }

    /// <summary>
    /// Initializes the view model.
    /// </summary>
    /// <returns><b>True</b>, if the initialization was successful, otherwise <b>false</b>.</returns>
    public override async Task<bool> InitializeAsync()
    {
        _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);

        // Check for existing versions
        ExistingVersions.Clear();
        var existingVersions = await _artifactsService.GetExistingArtifactVersionsAsync(_project, _configuration);
        if (existingVersions.Any())
        {
            foreach (var existingVersion in existingVersions)
                ExistingVersions.Add(existingVersion);
            SelectedBaseVersion = existingVersions.Single(m => m.IsNewestVersion);
        }
        else
        {
            SelectedBaseVersion = null;
        }

        // Check for scaffolding or creation mode
        ScaffoldingMode = ExistingVersions.Count == 0;

        InitializedOnce = true;
        return true;
    }

    private void EvaluateCommands()
    {
        ScaffoldDevelopmentVersionCommand.RaiseCanExecuteChanged();
        ScaffoldCurrentProductionVersionCommand.RaiseCanExecuteChanged();
        StartLatestCreationCommand.RaiseCanExecuteChanged();
        StartVersionedCreationCommand.RaiseCanExecuteChanged();
    }

    private async void ConfigurationService_ConfigurationChanged(object sender, ProjectConfigurationChangedEventArgs e)
    {
        if (e.Project.FullName != _project.FullName)
            return;

        _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);
        EvaluateCommands();
    }

    private void ScaffoldingService_IsScaffoldingChanged(object sender, EventArgs e)
    {
        EvaluateCommands();
    }

    private void ScriptCreationService_IsCreatingChanged(object sender, EventArgs e)
    {
        EvaluateCommands();
    }

    async Task IErrorHandler.HandleErrorAsync(IAsyncCommand command, Exception exception)
    {
        string commandName = null;
        if (ReferenceEquals(command, ScaffoldDevelopmentVersionCommand))
            commandName = nameof(ScaffoldDevelopmentVersionCommand);
        else if(ReferenceEquals(command, ScaffoldCurrentProductionVersionCommand))
            commandName = nameof(ScaffoldCurrentProductionVersionCommand);
        else if (ReferenceEquals(command, StartLatestCreationCommand))
            commandName = nameof(StartLatestCreationCommand);
        else if (ReferenceEquals(command, StartVersionedCreationCommand))
            commandName = nameof(StartVersionedCreationCommand);

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