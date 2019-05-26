namespace SSDTLifecycleExtension.ViewModels
{
    using System.Threading.Tasks;
    using Annotations;
    using EnvDTE;
    using Models;
    using Services;

    [UsedImplicitly]
    public class ConfigurationViewModel : ViewModelBase
    {
        private readonly Project _project;
        private readonly IConfigurationService _configurationService;

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

        public ConfigurationViewModel(Project project,
                                      IConfigurationService configurationService)
        {
            _project = project;
            _configurationService = configurationService;
        }

        public async Task InitializeAsync()
        {
            Model = await _configurationService.GetConfigurationOrDefaultAsync(_project);
        }
    }
}