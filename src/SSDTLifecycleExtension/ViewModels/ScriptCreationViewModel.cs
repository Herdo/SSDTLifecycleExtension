namespace SSDTLifecycleExtension.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using MVVM;
    using Shared.Contracts;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using Shared.Events;
    using Shared.Models;

    [UsedImplicitly]
    public class ScriptCreationViewModel : ViewModelBase,
                                           IErrorHandler
    {
        private readonly SqlProject _project;
        private readonly IConfigurationService _configurationService;
        private readonly IScaffoldingService _scaffoldingService;
        private readonly IScriptCreationService _scriptCreationService;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;
        private readonly ILogger _logger;

        private ConfigurationModel _configuration;

        private bool _scaffolding;
        private VersionModel _selectedBaseVersion;
        private bool _isCreatingScript;

        public bool Scaffolding
        {
            get => _scaffolding;
            set
            {
                if (value == _scaffolding)
                    return;
                _scaffolding = value;
                OnPropertyChanged();
            }
        }

        public VersionModel SelectedBaseVersion
        {
            get => _selectedBaseVersion;
            set
            {
                if (Equals(value, _selectedBaseVersion))
                    return;
                _selectedBaseVersion = value;
                OnPropertyChanged();
            }
        }

        public bool IsCreatingScript
        {
            get => _isCreatingScript;
            private set
            {
                if (value == _isCreatingScript)
                    return;
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
                                       IVisualStudioAccess visualStudioAccess,
                                       IFileSystemAccess fileSystemAccess,
                                       ILogger logger)
        {
            _project = project;
            _configurationService = configurationService;
            _scaffoldingService = scaffoldingService;
            _scriptCreationService = scriptCreationService;
            _visualStudioAccess = visualStudioAccess;
            _fileSystemAccess = fileSystemAccess;
            _logger = logger;

            ExistingVersions = new ObservableCollection<VersionModel>();

            ScaffoldDevelopmentVersionCommand = new AsyncCommand(ScaffoldDevelopmentVersion_ExecutedAsync, ScaffoldDevelopmentVersion_CanExecute, this);
            ScaffoldCurrentProductionVersionCommand = new AsyncCommand(ScaffoldCurrentProductionVersion_ExecutedAsync, ScaffoldCurrentProductionVersion_CanExecute, this);
            StartLatestCreationCommand = new AsyncCommand(StartLatestCreation_ExecutedAsync, StartLatestCreation_CanExecute, this);
            StartVersionedCreationCommand = new AsyncCommand(StartVersionedCreation_ExecutedAsync, StartVersionedCreation_CanExecute, this);

            _configurationService.ConfigurationChanged += ConfigurationService_ConfigurationChanged;
            _scaffoldingService.IsScaffoldingChanged += ScaffoldingService_IsScaffoldingChanged;
            _scriptCreationService.IsCreatingChanged += ScriptCreationService_IsCreatingChanged;
        }

        private async Task ScaffoldInternalAsync(Version targetVersion)
        {
            var successful = await _scaffoldingService.ScaffoldAsync(_project, _configuration, targetVersion, CancellationToken.None);
            if (successful)
            {
                await InitializeAsync();
            }
            else
            {
                ScaffoldDevelopmentVersionCommand.RaiseCanExecuteChanged();
                ScaffoldCurrentProductionVersionCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task CreateScriptInternalAsync(bool latest)
        {
            IsCreatingScript = true;
            try
            {
                var successful = await _scriptCreationService.CreateAsync(_project, _configuration, SelectedBaseVersion.UnderlyingVersion, latest, CancellationToken.None);
                if (successful)
                {
                    await InitializeAsync();
                }
                else
                {
                    StartLatestCreationCommand.RaiseCanExecuteChanged();
                    StartVersionedCreationCommand.RaiseCanExecuteChanged();
                }
            }
            finally
            {
                IsCreatingScript = false;
            }
        }

        private bool ScaffoldDevelopmentVersion_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

        private async Task ScaffoldDevelopmentVersion_ExecutedAsync()
        {
            await ScaffoldInternalAsync(new Version(0, 0, 0, 0));
        }

        private bool ScaffoldCurrentProductionVersion_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

        private async Task ScaffoldCurrentProductionVersion_ExecutedAsync()
        {
            await ScaffoldInternalAsync(new Version(1, 0, 0, 0));
        }

        private bool StartLatestCreation_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

        private async Task StartLatestCreation_ExecutedAsync()
        {
            await CreateScriptInternalAsync(true);
        }

        private bool StartVersionedCreation_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

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
            var projectPath = _project.FullName;
            var projectDirectory = Path.GetDirectoryName(projectPath);
            if (projectDirectory == null)
            {
                _visualStudioAccess.ShowModalError("ERROR: Cannot determine project directory.");
                return false;
            }
            var artifactsPath = Path.Combine(projectDirectory, _configuration.ArtifactsPath);
            string[] artifactDirectories;
            try
            {
                artifactDirectories = _fileSystemAccess.GetDirectoriesIn(artifactsPath);
            }
            catch (Exception e)
            {
                _visualStudioAccess.ShowModalError($"ERROR: Failed to open script creation window: {e.Message}");
                return false;
            }
            var existingVersions = new List<Version>();
            foreach (var artifactDirectory in artifactDirectories)
            {
                var di = new DirectoryInfo(artifactDirectory);
                if (Version.TryParse(di.Name, out var existingVersion))
                    existingVersions.Add(existingVersion);
            }

            SelectedBaseVersion = null;
            ExistingVersions.Clear();
            foreach (var version in existingVersions.OrderByDescending(m => m))
                ExistingVersions.Add(new VersionModel
                {
                    UnderlyingVersion = version
                });
            if (existingVersions.Any())
            {
                var highestVersion = existingVersions.Max();
                var highestModel = ExistingVersions.Single(m => m.UnderlyingVersion == highestVersion);
                highestModel.IsNewestVersion = true;
                SelectedBaseVersion = highestModel;
            }

            // Check for scaffolding or creation mode
            Scaffolding = ExistingVersions.Count == 0;

            // Evaluate commands
            ScaffoldDevelopmentVersionCommand.RaiseCanExecuteChanged();
            ScaffoldCurrentProductionVersionCommand.RaiseCanExecuteChanged();
            StartLatestCreationCommand.RaiseCanExecuteChanged();

            // If the configuration has errors, notify the user
            if (_configuration.HasErrors)
                _visualStudioAccess.ShowModalError("The SSDT Lifecycle configuration for this project is not correct. " +
                                                   "Please verify that the SSDT Lifecycle configuration for this project exists and has no errors.");

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
            if (e.Project.UniqueName != _project.UniqueName)
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

        void IErrorHandler.HandleError(IAsyncCommand command, Exception exception)
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

            if (commandName == null)
                throw new NotSupportedException();

            try
            {
                _logger.LogAsync($"Error during execution of {commandName}: {exception}").RunSynchronously();
            }
            catch
            {
                // ignored - when logging the exception fails, we don't want to end up in a stack overflow.
            }
        }
    }
}