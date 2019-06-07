namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;

    public interface ICommandAvailabilityService
    {
        void HandleCommandAvailability(Action<bool> setVisible,
                                       Action<bool> setEnabled);
    }
}