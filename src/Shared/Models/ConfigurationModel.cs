namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class ConfigurationModel : BaseModel
    {
        public const string SqlPackageSpecialKeyword = "{DEFAULT_LATEST_VERSION}";
        public const string MajorVersionSpecialKeyword = "{MAJOR}";
        public const string MinorVersionSpecialKeyword = "{MINOR}";
        public const string BuildVersionSpecialKeyword = "{BUILD}";
        public const string RevisionVersionSpecialKeyword = "{REVISION}";
        
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
                SetValidationErrors(ValidateArtifactsPath(_artifactsPath));
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
                SetValidationErrors(ValidatePublishProfilePath(_publishProfilePath));
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
                SetValidationErrors(ValidateCommentOutUnnamedDefaultConstraintDrops(value));
                SetValidationErrors(ValidateReplaceUnnamedDefaultConstraintDrops(ReplaceUnnamedDefaultConstraintDrops, nameof(ReplaceUnnamedDefaultConstraintDrops)), nameof(ReplaceUnnamedDefaultConstraintDrops));
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
                SetValidationErrors(ValidateReplaceUnnamedDefaultConstraintDrops(value));
                SetValidationErrors(ValidateCommentOutUnnamedDefaultConstraintDrops(CommentOutUnnamedDefaultConstraintDrops, nameof(CommentOutUnnamedDefaultConstraintDrops)), nameof(CommentOutUnnamedDefaultConstraintDrops));
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
                SetValidationErrors(ValidateVersionPattern(_versionPattern));
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
                SqlPackagePath = SqlPackageSpecialKeyword,
                PublishProfilePath = null,
                BuildBeforeScriptCreation = true,
                CreateDocumentationWithScriptCreation = true,
                CommentOutReferencedProjectRefactorings = false,
                CommentOutUnnamedDefaultConstraintDrops = false,
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "{MAJOR}.{MINOR}.{BUILD}",
                CustomHeader = null,
                CustomFooter = null
            };

        public void ValidateAll()
        {
            SetValidationErrors(ValidateArtifactsPath(ArtifactsPath, nameof(ArtifactsPath)), nameof(ArtifactsPath));
            SetValidationErrors(ValidateSqlPackagePath(SqlPackagePath, nameof(SqlPackagePath)), nameof(SqlPackagePath));
            SetValidationErrors(ValidatePublishProfilePath(PublishProfilePath, nameof(PublishProfilePath)), nameof(PublishProfilePath));
            SetValidationErrors(ValidateCommentOutUnnamedDefaultConstraintDrops(CommentOutUnnamedDefaultConstraintDrops, nameof(CommentOutUnnamedDefaultConstraintDrops)), nameof(CommentOutUnnamedDefaultConstraintDrops));
            SetValidationErrors(ValidateReplaceUnnamedDefaultConstraintDrops(ReplaceUnnamedDefaultConstraintDrops, nameof(ReplaceUnnamedDefaultConstraintDrops)), nameof(ReplaceUnnamedDefaultConstraintDrops));
            SetValidationErrors(ValidateVersionPattern(VersionPattern, nameof(VersionPattern)), nameof(VersionPattern));
        }

        private List<string> ValidateArtifactsPath(string value, [CallerMemberName] string propertyName = null)
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
                try
                {
                    if (Path.IsPathRooted(value))
                        errors.Add("Path must be a relative path.");
                }
                catch (ArgumentException)
                {
                    errors.Add("Path contains invalid characters.");
                }
            }

            return errors;
        }

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
                if (value == SqlPackageSpecialKeyword)
                    return errors;

                const string execName = "SqlPackage.exe";
                if (!value.EndsWith(execName))
                {
                    errors.Add($"Executable must be '{execName}' or the special keyword.");
                }
                else
                {
                    try
                    {
                        if (!Path.IsPathRooted(value))
                            errors.Add("Path must be an absolute path.");
                    }
                    catch (ArgumentException)
                    {
                        errors.Add("Path contains invalid characters.");
                    }
                }
            }

            return errors;
        }

        private List<string> ValidatePublishProfilePath(string value, [CallerMemberName] string propertyName = null)
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
                const string publishProfileExtension = ".publish.xml";
                if (!value.EndsWith(publishProfileExtension) || value.Length == publishProfileExtension.Length)
                    errors.Add($"Profile file name must end with *{publishProfileExtension}.");

                try
                {
                    if (Path.IsPathRooted(value))
                        errors.Add("Path must be a relative path.");
                }
                catch (ArgumentException)
                {
                    errors.Add("Path contains invalid characters.");
                }
            }

            return errors;
        }

        private List<string> ValidateCommentOutUnnamedDefaultConstraintDrops(bool value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var errors = new List<string>();

            if (value && ReplaceUnnamedDefaultConstraintDrops)
                errors.Add("Behavior for unnamed default constraint drops is ambiguous.");

            return errors;
        }

        private List<string> ValidateReplaceUnnamedDefaultConstraintDrops(bool value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var errors = new List<string>();

            if (value && CommentOutUnnamedDefaultConstraintDrops)
                errors.Add("Behavior for unnamed default constraint drops is ambiguous.");

            return errors;
        }

        private List<string> ValidateVersionPattern(string value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add("Pattern cannot be empty.");
            }
            else
            {
                var split = value.Split(new []{'.'}, StringSplitOptions.None);
                if (split.Length > 4) errors.Add("Pattern contains too many parts.");
                for (var i = 0; i < split.Length; i++)
                {
                    var isMajor = i == 0;
                    var isMinor = i == 1;
                    var isBuild = i == 2;
                    var isRevision = i == 3;

                    if (int.TryParse(split[i], out var number))
                    {
                        if (number >= 0) continue;

                        if (isMajor)
                            errors.Add("Major number cannot be negative.");
                        else if (isMinor)
                            errors.Add("Minor number cannot be negative.");
                        else if (isBuild)
                            errors.Add("Build number cannot be negative.");
                        else if (isRevision)
                            errors.Add("Revision number cannot be negative.");
                    }
                    else
                    {
                        if (isMajor && split[i] != MajorVersionSpecialKeyword)
                            errors.Add("Invalid special keyword for major number.");
                        else if (isMinor && split[i] != MinorVersionSpecialKeyword)
                            errors.Add("Invalid special keyword for minor number.");
                        else if (isBuild && split[i] != BuildVersionSpecialKeyword)
                            errors.Add("Invalid special keyword for build number.");
                        else if (isRevision && split[i] != RevisionVersionSpecialKeyword)
                            errors.Add("Invalid special keyword for revision number.");
                    }
                }

            }

            return errors;
        }
    }
}