namespace SSDTLifecycleExtension.Shared.Models;

public abstract class BaseModel : IBaseModel
{
    private readonly Dictionary<string, ICollection<string>> _validationErrors;

    protected BaseModel()
    {
        _validationErrors = new Dictionary<string, ICollection<string>>();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public IEnumerable GetErrors(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            return Array.Empty<string>();
        return _validationErrors.TryGetValue(propertyName, out var errors)
            ? errors
            : Array.Empty<string>();
    }

    [JsonIgnore] public bool HasErrors => _validationErrors.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    /// <summary>
    ///     Sets the <paramref name="validationErrors" /> for the <paramref name="propertyName" />.
    /// </summary>
    /// <param name="validationErrors">The list of validation errors.</param>
    /// <param name="propertyName">The name of the property to set the <paramref name="validationErrors" /> for.</param>
    /// <exception cref="ArgumentNullException"><paramref name="propertyName" /> is <b>null</b>.</exception>
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
}