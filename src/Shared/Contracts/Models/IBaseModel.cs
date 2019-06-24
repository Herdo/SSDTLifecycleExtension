namespace SSDTLifecycleExtension.Shared.Contracts.Models
{
    using System.ComponentModel;

    public interface IBaseModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        
    }
}