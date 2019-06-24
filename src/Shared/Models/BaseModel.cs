namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Contracts.Models;
    using JetBrains.Annotations;
    using Newtonsoft.Json;

    public abstract class BaseModel : IBaseModel
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
                return new string[0];
            return _validationErrors.TryGetValue(propertyName, out var errors)
                       ? errors
                       : new string[0];
        }

        [JsonIgnore]
        public bool HasErrors => _validationErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        /// <summary>
        /// Sets the <paramref name="validationErrors"/> for the <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="validationErrors">The list of validation errors.</param>
        /// <param name="propertyName">The name of the property to set the <paramref name="validationErrors"/> for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="propertyName"/> is <b>null</b>.</exception>
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