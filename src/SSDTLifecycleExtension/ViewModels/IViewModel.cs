namespace SSDTLifecycleExtension.ViewModels
{
    using System.ComponentModel;
    using System.Threading.Tasks;

    public interface IViewModel : INotifyPropertyChanged
    {
        Task<bool> InitializeAsync();
    }
}