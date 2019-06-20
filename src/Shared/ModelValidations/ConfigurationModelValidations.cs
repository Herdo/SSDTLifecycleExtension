namespace SSDTLifecycleExtension.Shared.ModelValidations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using JetBrains.Annotations;
    using Models;

    public static class ConfigurationModelValidations
    {
        /// <summary>
        /// Validates the <paramref name="model"/>.<see cref="ConfigurationModel.ArtifactsPath"/> and returns all found errors.
        /// </summary>
        /// <param name="model">The <see cref="ConfigurationModel"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <b>null</b>.</exception>
        /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
        [NotNull]
        public static List<string> ValidateArtifactsPath([NotNull] ConfigurationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.ArtifactsPath))
            {
                errors.Add("Path cannot be empty.");
            }
            else
            {
                try
                {
                    if (Path.IsPathRooted(model.ArtifactsPath))
                        errors.Add("Path must be a relative path.");
                }
                catch (ArgumentException)
                {
                    errors.Add("Path contains invalid characters.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates the <paramref name="model"/>.<see cref="ConfigurationModel.PublishProfilePath"/> and returns all found errors.
        /// </summary>
        /// <param name="model">The <see cref="ConfigurationModel"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <b>null</b>.</exception>
        /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
        [NotNull]
        public static List<string> ValidatePublishProfilePath([NotNull] ConfigurationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.PublishProfilePath))
            {
                errors.Add("Path cannot be empty.");
            }
            else
            {
                const string publishProfileExtension = ".publish.xml";
                if (!model.PublishProfilePath.EndsWith(publishProfileExtension) || model.PublishProfilePath.Length == publishProfileExtension.Length)
                    errors.Add($"Profile file name must end with *{publishProfileExtension}.");

                try
                {
                    if (Path.IsPathRooted(model.PublishProfilePath))
                        errors.Add("Path must be a relative path.");
                }
                catch (ArgumentException)
                {
                    errors.Add("Path contains invalid characters.");
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates the <paramref name="model"/>.<see cref="ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops"/> against the
        /// <paramref name="model"/>.<see cref="ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops"/> and returns all found errors.
        /// </summary>
        /// <param name="model">The <see cref="ConfigurationModel"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <b>null</b>.</exception>
        /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
        [NotNull]
        public static List<string> ValidateUnnamedDefaultConstraintDropsBehavior([NotNull] ConfigurationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var errors = new List<string>();

            if (model.CommentOutUnnamedDefaultConstraintDrops && model.ReplaceUnnamedDefaultConstraintDrops)
                errors.Add("Behavior for unnamed default constraint drops is ambiguous.");

            return errors;
        }

        /// <summary>
        /// Validates the <paramref name="model"/>.<see cref="ConfigurationModel.VersionPattern"/> and returns all found errors.
        /// </summary>
        /// <param name="model">The <see cref="ConfigurationModel"/> instance to validate.</param>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <b>null</b>.</exception>
        /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
        [NotNull]
        public static List<string> ValidateVersionPattern([NotNull] ConfigurationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(model.VersionPattern))
            {
                errors.Add("Pattern cannot be empty.");
            }
            else
            {
                var split = model.VersionPattern.Split(new[] { '.' }, StringSplitOptions.None);
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
            switch (position)
            {
                case 0 when number < 0:
                    errors.Add("Major number cannot be negative.");
                    break;
                case 1 when number < 0:
                    errors.Add("Minor number cannot be negative.");
                    break;
                case 2 when number < 0:
                    errors.Add("Build number cannot be negative.");
                    break;
                case 3 when number < 0:
                    errors.Add("Revision number cannot be negative.");
                    break;
                default:
                    return;
            }
        }

        private static void ValidateVersionNumberSpecialKeywords(IReadOnlyList<string> split,
                                                                 int position,
                                                                 ICollection<string> errors)
        {
            switch (position)
            {
                case 0 when split[position] != ConfigurationModel.MajorVersionSpecialKeyword:
                    errors.Add("Invalid special keyword for major number.");
                    break;
                case 1 when split[position] != ConfigurationModel.MinorVersionSpecialKeyword:
                    errors.Add("Invalid special keyword for minor number.");
                    break;
                case 2 when split[position] != ConfigurationModel.BuildVersionSpecialKeyword:
                    errors.Add("Invalid special keyword for build number.");
                    break;
                case 3 when split[position] != ConfigurationModel.RevisionVersionSpecialKeyword:
                    errors.Add("Invalid special keyword for revision number.");
                    break;
                default:
                    return;
            }
        }
    }
}