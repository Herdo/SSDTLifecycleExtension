﻿namespace SSDTLifecycleExtension.Shared.Contracts.Models;

public interface IStateModel : IBaseModel
{
    Func<bool, Task> HandleWorkInProgressChanged { get; }

    StateModelState CurrentState { get; set; }

    bool? Result { get; set; }
}