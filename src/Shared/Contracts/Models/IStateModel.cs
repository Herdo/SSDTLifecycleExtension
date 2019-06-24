namespace SSDTLifecycleExtension.Shared.Contracts.Models
{
    using System;
    using System.Threading.Tasks;
    using Enums;
    using JetBrains.Annotations;

    public interface IStateModel : IBaseModel
    {
        [NotNull]
        Func<bool, Task> HandleWorkInProgressChanged { get; }

        StateModelState CurrentState { get; set; }

        bool? Result { get; set; }
    }
}