#nullable enable

namespace SSDTLifecycleExtension.ViewModels;

public abstract class ViewModelBase : IViewModel
{
    public abstract Task<bool> InitializeAsync();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}