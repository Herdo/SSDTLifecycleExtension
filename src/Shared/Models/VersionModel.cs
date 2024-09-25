namespace SSDTLifecycleExtension.Shared.Models;

public sealed class VersionModel : BaseModel
{
    private Version _underlyingVersion;
    private bool _isNewestVersion;

    public Version UnderlyingVersion
    {
        get => _underlyingVersion;
        set
        {
            if (Equals(value, _underlyingVersion)) return;
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
            if (value == _isNewestVersion) return;
            _isNewestVersion = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayName));
        }
    }

    [UsedImplicitly] // Used from XAML as display name for the model.
    public string DisplayName
    {
        get
        {
            if (UnderlyingVersion == null)
                return "<null>";
            return IsNewestVersion
                ? $"{UnderlyingVersion} (newest)"
                : UnderlyingVersion.ToString();
        }
    }

    public VersionModel()
    {
        UnderlyingVersion = new Version();
    }
}