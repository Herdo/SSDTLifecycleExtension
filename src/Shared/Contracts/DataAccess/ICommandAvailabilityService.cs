namespace SSDTLifecycleExtension.Shared.Contracts.DataAccess
{
    using System;

    public interface ICommandAvailabilityService
    {
        void HandleCommandAvailability(object sender,
                                       EventArgs e);
    }
}