namespace SSDTLifecycleExtension.ViewModels
{
    using System.Threading.Tasks;
    using System.Windows.Input;
    using Annotations;
    using DataAccess;
    using EnvDTE;
    using Microsoft.VisualStudio.PlatformUI;
    using Services;
    using Shared.Models;

    [UsedImplicitly]
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly Project _project;
        private readonly IConfigurationService _configurationService;
        private readonly IFileSystemAccess _fileSystemAccess;

        private ConfigurationModel _model;

        public ConfigurationModel Model
        {
            get => _model;
            set
            {
                if (Equals(value, _model)) return;
                _model = value;
                OnPropertyChanged();
            }
        }

        public ICommand BrowseSqlPackageCommand { get; }
        public ICommand BrowsePublishProfileCommand { get; }
        public ICommand ResetConfigurationToDefaultCommand { get; }
        public ICommand SaveConfigurationCommand { get; }

        public ConfigurationViewModel(Project project,
                                      IConfigurationService configurationService,
                                      IFileSystemAccess fileSystemAccess)
        {
            _project = project;
            _configurationService = configurationService;
            _fileSystemAccess = fileSystemAccess;

            BrowseSqlPackageCommand = new DelegateCommand(BrowseSqlPackage_Executed);
            BrowsePublishProfileCommand = new DelegateCommand(BrowsePublishProfile_Executed);
            ResetConfigurationToDefaultCommand = new DelegateCommand(ResetConfigurationToDefault_Executed);
            SaveConfigurationCommand = new DelegateCommand(SaveConfiguration_Executed, SaveConfiguration_CanExecute);
        }

        private void BrowseSqlPackage_Executed(object obj)
        {
            var browsedPath = _fileSystemAccess.BrowseForFile(".exe", "Executable file (*.exe)|*.exe");
            if (browsedPath != null) Model.SqlPackagePath = browsedPath;
        }

        private void BrowsePublishProfile_Executed(object obj)
        {
            var browsedPath = _fileSystemAccess.BrowseForFile(".publish.xml", "Publish profile (*.publish.xml)|*.publish.xml");
            if (browsedPath != null) Model.PublishProfilePath = browsedPath;
        }

        private void ResetConfigurationToDefault_Executed(object obj)
        {
            Model = ConfigurationModel.GetDefault();
            Model.ValidateAll();
            if (SaveConfigurationCommand.CanExecute(null))
                SaveConfigurationCommand.Execute(null);
        }

        private bool SaveConfiguration_CanExecute(object obj)
        {
            return Model != null
                && !Model.HasErrors;
        }

        private async void SaveConfiguration_Executed(object obj)
        {
            await _configurationService.SaveConfigurationAsync(_project, Model);
        }

        public async Task InitializeAsync()
        {
            Model = await _configurationService.GetConfigurationOrDefaultAsync(_project);
        }
    }
}