namespace SSDTLifecycleExtension.Shared.Models
{
    using System;

    public class VersionModel : BaseModel
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

        public string DisplayName =>
            UnderlyingVersion == null
                ? "<null>"
                : IsNewestVersion
                    ? $"{UnderlyingVersion} (newest)"
                    : UnderlyingVersion.ToString();
    }
}