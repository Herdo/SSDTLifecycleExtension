namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Threading.Tasks;
    using Contracts.Enums;
    using Contracts.Models;
    using JetBrains.Annotations;

    public abstract class BaseStateModel : BaseModel, IStateModel
    {
        protected BaseStateModel([NotNull] Func<bool, Task> handleWorkInProgressChanged)
        {
            HandleWorkInProgressChanged = handleWorkInProgressChanged ?? throw new ArgumentNullException(nameof(handleWorkInProgressChanged));
            CurrentState = StateModelState.Initialized;
        }
        
        public Func<bool, Task> HandleWorkInProgressChanged { get; }

        public StateModelState CurrentState { get; set; }

        public bool? Result { get; set; }
    }
}