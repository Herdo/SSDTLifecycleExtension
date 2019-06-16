namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Services;
    using Contracts;
    using Contracts.DataAccess;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class ScaffoldingService : IScaffoldingService
    {
        private readonly ISqlProjectService _sqlProjectService;
        private readonly IBuildService _buildService;
        private readonly IVersionService _versionService;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly ILogger _logger;

        private bool _isScaffolding;

        public ScaffoldingService(ISqlProjectService sqlProjectService,
                                  IBuildService buildService,
                                  IVersionService versionService,
                                  IVisualStudioAccess visualStudioAccess,
                                  ILogger logger)
        {
            _sqlProjectService = sqlProjectService ?? throw new ArgumentNullException(nameof(sqlProjectService));
            _buildService = buildService ?? throw new ArgumentNullException(nameof(buildService));
            _versionService = versionService ?? throw new ArgumentNullException(nameof(versionService));
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> ShouldCancelAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            await _logger.LogAsync("Creation was canceled by the user.");
            return true;
        }

        private async Task<bool> ScaffoldInternalAsync(SqlProject project,
                                                 ConfigurationModel configuration,
                                                 Version targetVersion,
                                                 CancellationToken cancellationToken)
        {
            IsScaffolding = true;
            try
            {
                await _visualStudioAccess.StartLongRunningTaskIndicatorAsync();
                await _visualStudioAccess.ClearSSDTLifecycleOutputAsync();
                await _logger.LogAsync("Initializing scaffolding ...");
                var sw = new Stopwatch();
                sw.Start();

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                var formattedTargetVersion = Version.Parse(_versionService.DetermineFinalVersion(targetVersion, configuration));

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                if (!await _sqlProjectService.TryLoadSqlProjectPropertiesAsync(project))
                    return false;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                // Check DacVersion against planned target version
                if (project.ProjectProperties.DacVersion != formattedTargetVersion)
                {
                    await _logger.LogAsync($"ERROR: DacVersion of SQL project ({project.ProjectProperties.DacVersion}) doesn't match target version ({formattedTargetVersion}).");
                    _visualStudioAccess.ShowModalError("Please change the DAC version in the SQL project settings (see output window).");
                    return false;
                }

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                var paths = await _sqlProjectService.TryLoadPathsAsync(project, configuration);
                if (paths == null)
                    return false;

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                await _buildService.BuildProjectAsync(project);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return false;

                // Copy build result
                if (!await _buildService.CopyBuildResultAsync(project, paths.NewDacpacDirectory))
                    return false;

                // No check for the cancellation token after the last action.
                // Completion
                sw.Stop();
                await _logger.LogAsync($"========== Scaffolding version {formattedTargetVersion} finished after {sw.ElapsedMilliseconds} milliseconds. ==========");
            }
            catch (Exception e)
            {
                try
                {
                    await _logger.LogAsync($"ERROR: Script creation failed: {e.Message}");
                }
                catch
                {
                    // ignored
                }
            }
            finally
            {
                try
                {
                    await _visualStudioAccess.StopLongRunningTaskIndicatorAsync();
                }
                catch
                {
                    // ignored
                }

                IsScaffolding = false;
            }

            return true;
        }

        public event EventHandler IsScaffoldingChanged;

        private bool IsScaffolding
        {
            get => _isScaffolding;
            set
            {
                if (value == _isScaffolding) return;
                _isScaffolding = value;
                IsScaffoldingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        bool IScaffoldingService.IsScaffolding => IsScaffolding;

        Task<bool> IScaffoldingService.ScaffoldAsync(SqlProject project,
                                                     ConfigurationModel configuration,
                                                     Version targetVersion,
                                                     CancellationToken cancellationToken)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (targetVersion == null)
                throw new ArgumentNullException(nameof(targetVersion));
            if (IsScaffolding)
                throw new InvalidOperationException($"Service is already running a {nameof(IScriptCreationService.CreateAsync)} task.");

            return ScaffoldInternalAsync(project, configuration, targetVersion, cancellationToken);
        }
    }
}