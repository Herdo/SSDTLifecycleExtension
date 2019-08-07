namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Models;
    using JetBrains.Annotations;

    public abstract class AsyncExecutorBase<TStateModel>
        where TStateModel : IStateModel
    {
        private readonly ILogger _logger;

        protected AsyncExecutorBase([NotNull] ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<bool> ShouldCancelAsync(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                return false;

            await _logger.LogInfoAsync("Creation was canceled by the user.");
            return true;
        }

        private IWorkUnit<TStateModel> GetNextWorkUnit(TStateModel stateModel)
        {
            if (stateModel.Result == false)
                return null; // If the previous work unit set the total result to false, don't provide any further steps.

            var workUnit = GetNextWorkUnitForStateModel(stateModel);
            if (workUnit != null)
                return workUnit;

            // No more work units to complete.
            stateModel.Result = true;
            return null;
        }

        protected async Task DoWorkAsync(TStateModel stateModel, CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            sw.Start();

            try
            {
                await stateModel.HandleWorkInProgressChanged.Invoke(true);
                await _logger.LogInfoAsync(GetOperationStartedMessage());

                IWorkUnit<TStateModel> workUnit;
                do
                {
                    if (await ShouldCancelAsync(cancellationToken))
                        break;

                    workUnit = GetNextWorkUnit(stateModel);
                    if (workUnit != null)
                        await workUnit.Work(stateModel, cancellationToken);

                } while (workUnit != null);

                sw.Stop();
                await _logger.LogInfoAsync(GetOperationCompletedMessage(stateModel, sw.ElapsedMilliseconds));
            }
            catch (Exception e)
            {
                sw.Stop();
                try
                {
                    await _logger.LogErrorAsync(e, GetOperationFailedMessage());
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
                    await stateModel.HandleWorkInProgressChanged.Invoke(false);
                }
                catch
                {
                    // ignored
                }
            }
        }

        protected abstract string GetOperationStartedMessage();

        protected abstract string GetOperationCompletedMessage(TStateModel stateModel, long elapsedMilliseconds);

        protected abstract string GetOperationFailedMessage();

        protected abstract IWorkUnit<TStateModel> GetNextWorkUnitForStateModel(TStateModel stateModel);
    }
}