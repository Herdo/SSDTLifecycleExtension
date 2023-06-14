namespace SSDTLifecycleExtension.ViewModels;

public abstract class ViewModelBase : IViewModel
{
    public abstract Task<bool> InitializeAsync();

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}