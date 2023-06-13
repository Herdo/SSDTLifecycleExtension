namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface ICommandAvailabilityService
{
    void HandleCommandAvailability(Action<bool> setVisible,
                                   Action<bool> setEnabled);
}