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

        private readonly Dictionary<string, ICollection<string>> _validationErrors;

        #endregion

        #region Constructors

        protected BaseModel()
        {
            _validationErrors = new Dictionary<string, ICollection<string>>();
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
            return _validationErrors.TryGetValue(propertyName, out var errors)
                       ? errors
                       : null;
        }

        public bool HasErrors => _validationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected void SetValidationErrors(ICollection<string> validationErrors,
                                           [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (validationErrors == null || validationErrors.Count == 0)
                _validationErrors.Remove(propertyName);
            else
                _validationErrors[propertyName] = validationErrors;
            OnErrorsChanged(propertyName);
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}