namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class BaseModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region Fields

        protected readonly Dictionary<string, ICollection<string>> ValidationErrors;

        #endregion

        #region Constructors

        protected BaseModel()
        {
            ValidationErrors = new Dictionary<string, ICollection<string>>();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region INotifyDataErrorInfo

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;
            return ValidationErrors.TryGetValue(propertyName, out var errors)
                       ? errors
                       : null;
        }

        public bool HasErrors => ValidationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected void OnErrorsChanged([CallerMemberName] string propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}