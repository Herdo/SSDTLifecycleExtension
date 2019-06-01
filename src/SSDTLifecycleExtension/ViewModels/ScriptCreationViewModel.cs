namespace SSDTLifecycleExtension.ViewModels
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Annotations;
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

        private ConfigurationModel _configuration;

        public ICommand StartCreationCommand { get; }

        public ScriptCreationViewModel(Project project,
                                       IConfigurationService configurationService,
                                       IScriptCreationService scriptCreationService)
        {
            _project = project;
            _configurationService = configurationService;
            _scriptCreationService = scriptCreationService;
            
            StartCreationCommand = new DelegateCommand(StartCreation_Executed);

            _configurationService.ConfigurationChanged += ConfigurationService_ConfigurationChanged;
        }

        private async void StartCreation_Executed()
        {
            await _scriptCreationService.CreateAsync(_project, _configuration, Version.Parse("0.0.0.0"), null, CancellationToken.None);
        }

        public async Task InitializeAsync()
        {
            _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);
        }

        private async void ConfigurationService_ConfigurationChanged(object sender, ProjectConfigurationChangedEventArgs e)
        {
            if (!ReferenceEquals(e.Project, _project))
                return;

            _configuration = await _configurationService.GetConfigurationOrDefaultAsync(_project);
            OnPropertyChanged(null);
        }
    }
}