namespace SSDTLifecycleExtension.ViewModels;

public interface IViewModel : INotifyPropertyChanged
{
    Task<bool> InitializeAsync();
}