namespace SSDTLifecycleExtension.Services
{
    using System;

    public interface ICommandAvailabilityService
    {
        void HandleCommandAvailability(object sender,
                                       EventArgs e);
    }
}