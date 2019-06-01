using System;
using System.Threading.Tasks;
using EnvDTE;
using SSDTLifecycleExtension.Shared.Models;

namespace SSDTLifecycleExtension.Services
{
    using System.IO;
    using Annotations;
    using DataAccess;

    [UsedImplicitly]
    public class ScriptCreationService : IScriptCreationService
    {
        private readonly IVisualStudioAccess _visualStudioAccess;

        private bool _isCreating;

        public ScriptCreationService(IVisualStudioAccess visualStudioAccess)
        {
            _visualStudioAccess = visualStudioAccess;
        }

        bool IScriptCreationService.IsCreating => _isCreating;

        async Task IScriptCreationService.CreateAsync(Project project,
                                                      ConfigurationModel configuration,
                                                      Version previousVersion,
                                                      Version newVersion)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (previousVersion == null)
                throw new ArgumentNullException(nameof(previousVersion));
            if (_isCreating)
                throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

            _isCreating = true;
            try
            {
                await _visualStudioAccess.ClearSSDTLifecycleOutputAsync();
                await _visualStudioAccess.WriteLineToSSDTLifecycleOutputAsync("Initializing script creation...");

                var createLatest = newVersion == null;
                var projectDirectory = Path.GetDirectoryName(project.FullName);
                if (projectDirectory == null)
                    throw new InvalidOperationException("Cannot get project directory.");
                var artifactsPath = Path.Combine(projectDirectory, configuration.ArtifactsPath);
                var profilePath = Path.Combine(projectDirectory, configuration.PublishProfilePath);
            }
            finally
            {
                _isCreating = false;
            }
        }
    }
}