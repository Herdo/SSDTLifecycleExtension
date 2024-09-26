namespace SSDTLifecycleExtension.Shared.Models;

public sealed class VersionModel : BaseModel
{
    private Version _underlyingVersion = new (0, 0);
    private bool _isNewestVersion;

    public Version UnderlyingVersion
    {
        get => _underlyingVersion;
        set
        {
            if (Equals(value, _underlyingVersion))
                return;
            _underlyingVersion = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    public bool IsNewestVersion
    {
        get => _isNewestVersion;
        set
        {
            if (value == _isNewestVersion)
                return;
            _isNewestVersion = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    /// <summary>
    /// Used from XAML as display name for the model.
    /// </summary>
    public string DisplayName
    {
        get
        {
            return IsNewestVersion
                ? $"{UnderlyingVersion} (newest)"
                : UnderlyingVersion.ToString();
        }
    }
}