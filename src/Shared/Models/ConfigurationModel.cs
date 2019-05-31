namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class ConfigurationModel : BaseModel
    {
        private const string _SQL_PACKAGE_SPECIAL_KEYWORD = "{DEFAULT_LATEST_VERSION}";
        
        private string _artifactsPath;
        private string _sqlPackagePath;
        private string _publishProfilePath;
        private bool _buildBeforeScriptCreation;
        private bool _createDocumentationWithScriptCreation;
        private bool _commentOutReferencedProjectRefactorings;
        private bool _commentOutUnnamedDefaultConstraintDrops;
        private bool _replaceUnnamedDefaultConstraintDrops;
        private string _versionPattern;
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
            }
        }

        /// <summary>
        /// Gets or sets the absolute SqlPackage.exe path.
        /// </summary>
        public string SqlPackagePath
        {
            get => _sqlPackagePath;
            set
            {
                if (value == _sqlPackagePath) return;
                _sqlPackagePath = value;
                OnPropertyChanged();
                SetValidationErrors(ValidateSqlPackagePath(_sqlPackagePath));
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
        /// Gets or sets whether to comment out refactorings of referenced projects, or not.
        /// </summary>
        public bool CommentOutReferencedProjectRefactorings
        {
            get => _commentOutReferencedProjectRefactorings;
            set
            {
                if (value == _commentOutReferencedProjectRefactorings) return;
                _commentOutReferencedProjectRefactorings = value;
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
                SqlPackagePath = _SQL_PACKAGE_SPECIAL_KEYWORD,
                PublishProfilePath = null,
                BuildBeforeScriptCreation = true,
                CreateDocumentationWithScriptCreation = true,
                CommentOutReferencedProjectRefactorings = false,
                CommentOutUnnamedDefaultConstraintDrops = false,
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "{MAJOR}.{MINOR}.{PATCH}",
                CustomHeader = null,
                CustomFooter = null
            };

        private List<string> ValidateSqlPackagePath(string value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add("Path cannot be empty.");
            }
            else
            {
                const string execName = "SqlPackage.exe";
                if (value != _SQL_PACKAGE_SPECIAL_KEYWORD && !value.EndsWith(execName))
                {
                    errors.Add($"Executable must be '{execName}' or the special keyword.");
                }
            }

            return errors;
        }
    }
}