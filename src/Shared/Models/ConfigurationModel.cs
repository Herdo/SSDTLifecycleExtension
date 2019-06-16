namespace SSDTLifecycleExtension.Shared.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class ConfigurationModel : BaseModel
    {
        public const string MajorVersionSpecialKeyword = "{MAJOR}";
        public const string MinorVersionSpecialKeyword = "{MINOR}";
        public const string BuildVersionSpecialKeyword = "{BUILD}";
        public const string RevisionVersionSpecialKeyword = "{REVISION}";
        
        private string _artifactsPath;
        private string _publishProfilePath;
        private bool _buildBeforeScriptCreation;
        private bool _createDocumentationWithScriptCreation;
        private bool _commentOutReferencedProjectRefactorings;
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
                SetValidationErrors(ValidateArtifactsPath(_artifactsPath));
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
                CommentOutReferencedProjectRefactorings = false,
                CommentOutUnnamedDefaultConstraintDrops = false,
                ReplaceUnnamedDefaultConstraintDrops = false,
                VersionPattern = "{MAJOR}.{MINOR}.{BUILD}",
                TrackDacpacVersion = false,
                CustomHeader = null,
                CustomFooter = null
            };

        public void ValidateAll()
        {
            SetValidationErrors(ValidateArtifactsPath(ArtifactsPath, nameof(ArtifactsPath)), nameof(ArtifactsPath));
            SetValidationErrors(ValidatePublishProfilePath(PublishProfilePath, nameof(PublishProfilePath)), nameof(PublishProfilePath));
            SetValidationErrors(ValidateCommentOutUnnamedDefaultConstraintDrops(CommentOutUnnamedDefaultConstraintDrops, nameof(CommentOutUnnamedDefaultConstraintDrops)), nameof(CommentOutUnnamedDefaultConstraintDrops));
            SetValidationErrors(ValidateReplaceUnnamedDefaultConstraintDrops(ReplaceUnnamedDefaultConstraintDrops, nameof(ReplaceUnnamedDefaultConstraintDrops)), nameof(ReplaceUnnamedDefaultConstraintDrops));
            SetValidationErrors(ValidateVersionPattern(VersionPattern, nameof(VersionPattern)), nameof(VersionPattern));
        }

        public ConfigurationModel Copy()
        {
            var copy = new ConfigurationModel
            {
                ArtifactsPath = ArtifactsPath,
                PublishProfilePath = PublishProfilePath,
                BuildBeforeScriptCreation = BuildBeforeScriptCreation,
                CreateDocumentationWithScriptCreation = CreateDocumentationWithScriptCreation,
                CommentOutReferencedProjectRefactorings = CommentOutReferencedProjectRefactorings,
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
                if (split.Length < 2) errors.Add("Pattern doesn't contain enough parts.");
                if (split.Length > 4) errors.Add("Pattern contains too many parts.");
                for (var position = 0; position < split.Length; position++)
                    ValidateVersionNumberPosition(split, position, errors);

            }

            return errors;
        }

        private static void ValidateVersionNumberPosition(IReadOnlyList<string> split,
                                                          int position,
                                                          ICollection<string> errors)
        {
            if (int.TryParse(split[position], out var number))
            {
                ValidateVersionNumberDigits(position, errors, number);
            }
            else
            {
                ValidateVersionNumberSpecialKeywords(split, position, errors);
            }
        }

        private static void ValidateVersionNumberDigits(int position,
                                                        ICollection<string> errors,
                                                        int number)
        {
            if (number >= 0) return;

            switch (position)
            {
                case 0:
                    errors.Add("Major number cannot be negative.");
                    break;
                case 1:
                    errors.Add("Minor number cannot be negative.");
                    break;
                case 2:
                    errors.Add("Build number cannot be negative.");
                    break;
                case 3:
                    errors.Add("Revision number cannot be negative.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateVersionNumberSpecialKeywords(IReadOnlyList<string> split,
                                                                 int position,
                                                                 ICollection<string> errors)
        {
            if (position == 0 && split[position] != MajorVersionSpecialKeyword)
                errors.Add("Invalid special keyword for major number.");
            else if (position == 1 && split[position] != MinorVersionSpecialKeyword)
                errors.Add("Invalid special keyword for minor number.");
            else if (position == 2 && split[position] != BuildVersionSpecialKeyword)
                errors.Add("Invalid special keyword for build number.");
            else if (position == 3 && split[position] != RevisionVersionSpecialKeyword)
                errors.Add("Invalid special keyword for revision number.");
        }
    }
}