namespace SSDTLifecycleExtension.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Annotations;
    using DataAccess;
    using EnvDTE;
    using Events;
    using Microsoft.VisualStudio.PlatformUI;
    using Services;
    using Shared.Models;

    [UsedImplicitly]
    public class ScriptCreationViewModel : ViewModelBase
    {
        private readonly Project _project;
        private readonly IConfigurationService _configurationService;
        private readonly IScriptCreationService _scriptCreationService;
        private readonly IFileSystemAccess _fileSystemAccess;

        private ConfigurationModel _configuration;

        private bool _scaffolding;

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

        public ObservableCollection<string> ExistingVersions { get; }

        public DelegateCommand StartCreationCommand { get; }

        public ScriptCreationViewModel(Project project,
                                       IConfigurationService configurationService,
                                       IScriptCreationService scriptCreationService,
                                       IFileSystemAccess fileSystemAccess)
        {
            _project = project;
            _configurationService = configurationService;
            _scriptCreationService = scriptCreationService;
            _fileSystemAccess = fileSystemAccess;

            ExistingVersions = new ObservableCollection<string>();

            StartCreationCommand = new DelegateCommand(StartCreation_Executed, StartCreation_CanExecute);

            _configurationService.ConfigurationChanged += ConfigurationService_ConfigurationChanged;
        }

        private bool StartCreation_CanExecute() => _configuration != null
                                                   && !_configuration.HasErrors
                                                   && !_scriptCreationService.IsCreating;

        private async void StartCreation_Executed()
        {
            await _scriptCreationService.CreateAsync(_project, _configuration, Version.Parse("0.0.0.0"), null, CancellationToken.None);
            StartCreationCommand.RaiseCanExecuteChanged();
        }

        public async Task InitializeAsync()
        {
            _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);

            // Check for existing versions
            var projectPath = _project.FullName;
            var projectDirectory = Path.GetDirectoryName(projectPath);
            if (projectDirectory == null)
                return;
            var artifactsPath = Path.Combine(projectDirectory, _configuration.ArtifactsPath);
            var artifactDirectories = _fileSystemAccess.GetDirectoriesIn(artifactsPath);
            var existingVersions = new List<Version>();
            foreach (var artifactDirectory in artifactDirectories)
            {
                var directoryName = Path.GetDirectoryName(artifactDirectory);
                if (directoryName != null && Version.TryParse(directoryName, out var existingVersion))
                    existingVersions.Add(existingVersion);
            }
            foreach (var version in existingVersions.OrderBy(m => m))
                ExistingVersions.Add(version.ToString());

            // Check for scaffolding or creation mode
            Scaffolding = ExistingVersions.Count == 0;

            // Evaluate commands
            StartCreationCommand.RaiseCanExecuteChanged();
        }

        private async void ConfigurationService_ConfigurationChanged(object sender, ProjectConfigurationChangedEventArgs e)
        {
            if (!ReferenceEquals(e.Project, _project))
                return;

            _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);
            StartCreationCommand.RaiseCanExecuteChanged();
        }
    }
}