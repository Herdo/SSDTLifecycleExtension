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
    using Microsoft.VisualStudio.PlatformUI;
    using Shared.Contracts;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using Shared.Events;
    using Shared.Models;

    [UsedImplicitly]
    public class ScriptCreationViewModel : ViewModelBase
    {
        private readonly SqlProject _project;
        private readonly IConfigurationService _configurationService;
        private readonly IScaffoldingService _scaffoldingService;
        private readonly IScriptCreationService _scriptCreationService;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly IFileSystemAccess _fileSystemAccess;

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

        public DelegateCommand ScaffoldDevelopmentVersionCommand { get; }

        public DelegateCommand ScaffoldCurrentProductionVersionCommand { get; }

        public DelegateCommand StartLatestCreationCommand { get; }

        public DelegateCommand StartVersionedCreationCommand { get; }

        public ScriptCreationViewModel(SqlProject project,
                                       IConfigurationService configurationService,
                                       IScaffoldingService scaffoldingService,
                                       IScriptCreationService scriptCreationService,
                                       IVisualStudioAccess visualStudioAccess,
                                       IFileSystemAccess fileSystemAccess)
        {
            _project = project;
            _configurationService = configurationService;
            _scaffoldingService = scaffoldingService;
            _scriptCreationService = scriptCreationService;
            _visualStudioAccess = visualStudioAccess;
            _fileSystemAccess = fileSystemAccess;

            ExistingVersions = new ObservableCollection<VersionModel>();

            ScaffoldDevelopmentVersionCommand = new DelegateCommand(ScaffoldDevelopmentVersion_Executed, ScaffoldDevelopmentVersion_CanExecute);
            ScaffoldCurrentProductionVersionCommand = new DelegateCommand(ScaffoldCurrentProductionVersion_Executed, ScaffoldCurrentProductionVersion_CanExecute);
            StartLatestCreationCommand = new DelegateCommand(StartLatestCreation_Executed, StartLatestCreation_CanExecute);
            StartVersionedCreationCommand = new DelegateCommand(StartVersionedCreation_Executed, StartVersionedCreation_CanExecute);

            _configurationService.ConfigurationChanged += ConfigurationService_ConfigurationChanged;
        }

        private async Task ScaffoldInternal(Version targetVersion)
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

        private async Task CreateScriptInternal(bool latest)
        {
            IsCreatingScript = true;
            try
            {
                await _scriptCreationService.CreateAsync(_project, _configuration, SelectedBaseVersion.UnderlyingVersion, latest, CancellationToken.None);
                StartLatestCreationCommand.RaiseCanExecuteChanged();
                StartVersionedCreationCommand.RaiseCanExecuteChanged();
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

        private async void ScaffoldDevelopmentVersion_Executed()
        {
            await ScaffoldInternal(new Version(0, 0, 0, 0));
        }

        private bool ScaffoldCurrentProductionVersion_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

        private async void ScaffoldCurrentProductionVersion_Executed()
        {
            await ScaffoldInternal(new Version(1, 0, 0, 0));
        }

        private bool StartLatestCreation_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

        private async void StartLatestCreation_Executed()
        {
            await CreateScriptInternal(true);
        }

        private bool StartVersionedCreation_CanExecute() =>
            _configuration != null
            && !_configuration.HasErrors
            && !_scaffoldingService.IsScaffolding
            && !_scriptCreationService.IsCreating;

        private async void StartVersionedCreation_Executed()
        {
            await CreateScriptInternal(false);
        }

        /// <summary>
        /// Initializes the view model.
        /// </summary>
        /// <returns><b>True</b>, if the initialization was successful, otherwise <b>false</b>.</returns>
        public async Task<bool> InitializeAsync()
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
            return true;
        }

        private async void ConfigurationService_ConfigurationChanged(object sender, ProjectConfigurationChangedEventArgs e)
        {
            if (!ReferenceEquals(e.Project, _project))
                return;

            _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);
            ScaffoldDevelopmentVersionCommand.RaiseCanExecuteChanged();
            ScaffoldCurrentProductionVersionCommand.RaiseCanExecuteChanged();
            StartLatestCreationCommand.RaiseCanExecuteChanged();
        }
    }
}