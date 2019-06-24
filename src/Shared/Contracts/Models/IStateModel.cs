namespace SSDTLifecycleExtension.Shared.Contracts.Models
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;

    public interface IStateModel : IBaseModel
    {
        [NotNull]
        Func<bool, Task> HandleWorkInProgressChanged { get; }

        bool? Result { get; set; }
    }
}