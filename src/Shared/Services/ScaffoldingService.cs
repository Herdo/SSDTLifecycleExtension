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
        private readonly IBuildService _buildService;
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly ILogger _logger;

        private bool _isScaffolding;

        public ScaffoldingService(IBuildService buildService,
                                  IVisualStudioAccess visualStudioAccess,
                                  ILogger logger)
        {
            _buildService = buildService;
            _visualStudioAccess = visualStudioAccess;
            _logger = logger;
        }

        private async Task<bool> ShouldCancelAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            await _logger.LogAsync("Creation was canceled by the user.");
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

        bool IScaffoldingService.IsScaffolding => throw new NotImplementedException();

        async Task IScaffoldingService.ScaffoldAsync(SqlProject project,
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

            IsScaffolding = true;
            try
            {
                await _visualStudioAccess.StartLongRunningTaskIndicatorAsync();
                await _visualStudioAccess.ClearSSDTLifecycleOutputAsync();
                await _logger.LogAsync("Initializing scaffolding ...");
                var sw = new Stopwatch();
                sw.Start();
                if (await ShouldCancelAsync(cancellationToken))
                    return;

                await _buildService.BuildProjectAsync(project);

                // Cancel if requested
                if (await ShouldCancelAsync(cancellationToken))
                    return;
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
        }
    }
}