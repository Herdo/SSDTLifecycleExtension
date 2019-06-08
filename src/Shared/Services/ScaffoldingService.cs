namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Services;
    using Contracts;
    using Contracts.DataAccess;
    using Models;

    public class ScaffoldingService : IScaffoldingService
    {
        private readonly IVisualStudioAccess _visualStudioAccess;
        private readonly ILogger _logger;

        private bool _isScaffolding;

        public ScaffoldingService(IVisualStudioAccess visualStudioAccess,
                                  ILogger logger)
        {
            _visualStudioAccess = visualStudioAccess;
            _logger = logger;
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