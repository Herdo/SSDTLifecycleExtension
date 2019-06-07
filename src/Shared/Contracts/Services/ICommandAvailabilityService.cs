namespace SSDTLifecycleExtension.Shared.Contracts.Services
{
    using System;

    public interface ICommandAvailabilityService
    {
        void HandleCommandAvailability(object sender,
                                       EventArgs e);
    }
}