namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using ModelValidations;

    public sealed class ConfigurationModel : BaseModel,
                                             IEquatable<ConfigurationModel>
    {
        public const string MajorVersionSpecialKeyword = "{MAJOR}";
        public const string MinorVersionSpecialKeyword = "{MINOR}";
        public const string BuildVersionSpecialKeyword = "{BUILD}";
        public const string RevisionVersionSpecialKeyword = "{REVISION}";

        private string _artifactsPath;
        private string _publishProfilePath;
        private bool _buildBeforeScriptCreation;
        private bool _createDocumentationWithScriptCreation;
        private bool _commentOutUnnamedDefaultConstraintDrops;
        private bool _replaceUnnamedDefaultConstraintDrops;
        private string _versionPattern;
        private bool _trackDacpacVersion;
        private string _customHeader;
        private string _customFooter;

        /// <summary>
        /// Gets or sets the relative artifacts path.
        /// </summary>
        /// <remarks>Path is relative to the *.sqlproj file.</remarks>
        public string ArtifactsPath
        {
            get => _artifactsPath;
            set
            {
                if (value == _artifactsPath) return;
                _artifactsPath = value;
                OnPropertyChanged();
                SetValidationErrors(ConfigurationModelValidations.ValidateArtifactsPath(this));
            }
        }

        /// <summary>
        /// Gets or sets the relative publish profile path.
        /// </summary>
        /// <remarks>Path is relative to the *.sqlproj file.</remarks>
        public string PublishProfilePath
        {
            get => _publishProfilePath;
            set
            {
                if (value == _publishProfilePath) return;
                _publishProfilePath = value;
                OnPropertyChanged();
                SetValidationErrors(ConfigurationModelValidations.ValidatePublishProfilePath(this));
            }
        }

        /// <summary>
        /// Gets or sets whether to build the database project before creating the script.
        /// </summary>
        public bool BuildBeforeScriptCreation
        {
            get => _buildBeforeScriptCreation;
            set
            {
                if (value == _buildBeforeScriptCreation) return;
                _buildBeforeScriptCreation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether script creation also includes the documentation creation.
        /// </summary>
        public bool CreateDocumentationWithScriptCreation
        {
            get => _createDocumentationWithScriptCreation;
            set
            {
                if (value == _createDocumentationWithScriptCreation) return;
                _createDocumentationWithScriptCreation = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether to comment out dropping unnamed default constraints, or not.
        /// </summary>
        public bool CommentOutUnnamedDefaultConstraintDrops
        {
            get => _commentOutUnnamedDefaultConstraintDrops;
            set
            {
                if (value == _commentOutUnnamedDefaultConstraintDrops) return;
                _commentOutUnnamedDefaultConstraintDrops = value;
                OnPropertyChanged();
                var errors = ConfigurationModelValidations.ValidateUnnamedDefaultConstraintDropsBehavior(this);
                SetValidationErrors(errors);
                SetValidationErrors(errors, nameof(ReplaceUnnamedDefaultConstraintDrops));
            }
        }

        /// <summary>
        /// Gets or sets whether to replace dropping unnamed default constraints with a proper dropping statement, or not.
        /// </summary>
        public bool ReplaceUnnamedDefaultConstraintDrops
        {
            get => _replaceUnnamedDefaultConstraintDrops;
            set
            {
                if (value == _replaceUnnamedDefaultConstraintDrops) return;
                _replaceUnnamedDefaultConstraintDrops = value;
                OnPropertyChanged();
                var errors = ConfigurationModelValidations.ValidateUnnamedDefaultConstraintDropsBehavior(this);
                SetValidationErrors(errors);
                SetValidationErrors(errors, nameof(CommentOutUnnamedDefaultConstraintDrops));
            }
        }

        /// <summary>
        /// Gets or sets the version pattern.
        /// </summary>
        public string VersionPattern
        {
            get => _versionPattern;
            set
            {
                if (value == _versionPattern) return;
                _versionPattern = value;
                OnPropertyChanged();
                SetValidationErrors(ConfigurationModelValidations.ValidateVersionPattern(this));
            }
        }

        /// <summary>
        /// Gets or sets whether the DACPAC version should be tracked in the [dbo].[__DacpacVersion] table.
        /// </summary>
        public bool TrackDacpacVersion
        {
            get => _trackDacpacVersion;
            set
            {
                if (value == _trackDacpacVersion) return;
                _trackDacpacVersion = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a custom header, that will be added at the beginning of the created script.
        /// </summary>
        public string CustomHeader
        {
            get => _customHeader;
            set
            {
                if (value == _customHeader) return;
                _customHeader = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a custom footer, that will be added at the end of the created script.
        /// </summary>
        public string CustomFooter
        {
            get => _customFooter;
            set
            {
                if (value == _customFooter) return;
                _customFooter = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the default configuration.
        /// </summary>
        /// <returns>A new <see cref="ConfigurationModel"/> instance.</returns>
        public static ConfigurationModel GetDefault() =>
            new ConfigurationModel
            {
                ArtifactsPath = "_Deployment",
                PublishProfilePath = null,
                BuildBeforeScriptCreation = true,
                CreateDocumentationWithScriptCreation = true,
                CommentOutUnnamedDefaultConstraintDrops = false,
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "{MAJOR}.{MINOR}.{BUILD}",
                TrackDacpacVersion = false,
                CustomHeader = null,
                CustomFooter = null
            };

        public void ValidateAll()
        {
            SetValidationErrors(ConfigurationModelValidations.ValidateArtifactsPath(this), nameof(ArtifactsPath));
            SetValidationErrors(ConfigurationModelValidations.ValidatePublishProfilePath(this), nameof(PublishProfilePath));

            var unnamedDefaultConstraintDropsErrors = ConfigurationModelValidations.ValidateUnnamedDefaultConstraintDropsBehavior(this);
            SetValidationErrors(unnamedDefaultConstraintDropsErrors, nameof(CommentOutUnnamedDefaultConstraintDrops));
            SetValidationErrors(unnamedDefaultConstraintDropsErrors, nameof(ReplaceUnnamedDefaultConstraintDrops));

            SetValidationErrors(ConfigurationModelValidations.ValidateVersionPattern(this), nameof(VersionPattern));
        }

        public ConfigurationModel Copy()
        {
            var copy = new ConfigurationModel
            {
                ArtifactsPath = ArtifactsPath,
                PublishProfilePath = PublishProfilePath,
                BuildBeforeScriptCreation = BuildBeforeScriptCreation,
                CreateDocumentationWithScriptCreation = CreateDocumentationWithScriptCreation,
                CommentOutUnnamedDefaultConstraintDrops = CommentOutUnnamedDefaultConstraintDrops,
                ReplaceUnnamedDefaultConstraintDrops = ReplaceUnnamedDefaultConstraintDrops,
                VersionPattern = VersionPattern,
                TrackDacpacVersion = TrackDacpacVersion,
                CustomHeader = CustomHeader,
                CustomFooter = CustomFooter
            };
            copy.ValidateAll();
            return copy;
        }

        public bool Equals(ConfigurationModel other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(_artifactsPath, other._artifactsPath)
                   && string.Equals(_publishProfilePath, other._publishProfilePath)
                   && _buildBeforeScriptCreation == other._buildBeforeScriptCreation
                   && _createDocumentationWithScriptCreation == other._createDocumentationWithScriptCreation
                   && _commentOutUnnamedDefaultConstraintDrops == other._commentOutUnnamedDefaultConstraintDrops
                   && _replaceUnnamedDefaultConstraintDrops == other._replaceUnnamedDefaultConstraintDrops
                   && string.Equals(_versionPattern, other._versionPattern)
                   && _trackDacpacVersion == other._trackDacpacVersion
                   && string.Equals(_customHeader, other._customHeader)
                   && string.Equals(_customFooter, other._customFooter);
        }
    }
}