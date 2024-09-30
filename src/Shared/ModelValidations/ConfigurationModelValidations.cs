namespace SSDTLifecycleExtension.Shared.ModelValidations;

public static class ConfigurationModelValidations
{
    /// <summary>
    ///     Validates the <paramref name="model" />.<see cref="ConfigurationModel.ArtifactsPath" /> and returns all found
    ///     errors.
    /// </summary>
    /// <param name="model">The <see cref="ConfigurationModel" /> instance to validate.</param>
    /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
    public static List<string> ValidateArtifactsPath(ConfigurationModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.ArtifactsPath))
            errors.Add("Path cannot be empty.");
        else
            try
            {
                if (Path.IsPathRooted(model.ArtifactsPath))
                    errors.Add("Path must be a relative path.");
            }
            catch (ArgumentException)
            {
                errors.Add("Path contains invalid characters.");
            }

        return errors;
    }

    /// <summary>
    ///     Validates the <paramref name="model" />.<see cref="ConfigurationModel.PublishProfilePath" /> and returns all found
    ///     errors.
    /// </summary>
    /// <param name="model">The <see cref="ConfigurationModel" /> instance to validate.</param>
    /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
    public static List<string> ValidatePublishProfilePath(ConfigurationModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.PublishProfilePath))
            errors.Add("Path cannot be empty.");
        else if (model.PublishProfilePath == ConfigurationModel.UseSinglePublishProfileSpecialKeyword)
            return errors;
        else
            ValidateActualPublishProfilePath(model, errors);

        return errors;
    }

    private static void ValidateActualPublishProfilePath(ConfigurationModel model,
        List<string> errors)
    {
        const string publishProfileExtension = ".publish.xml";
        if (string.IsNullOrWhiteSpace(model.PublishProfilePath)
         || !model.PublishProfilePath!.EndsWith(publishProfileExtension)
         || model.PublishProfilePath.Length == publishProfileExtension.Length)
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

    /// <summary>
    ///     Validates the <paramref name="model" />.<see cref="ConfigurationModel.SharedDacpacRepositoryPath" /> and returns
    ///     all found errors.
    /// </summary>
    /// <param name="model">The <see cref="ConfigurationModel" /> instance to validate.</param>
    /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
    public static List<string> ValidateSharedDacpacRepositoryPath(ConfigurationModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.SharedDacpacRepositoryPath))
            return errors;

        try
        {
            if (!Path.IsPathRooted(model.SharedDacpacRepositoryPath))
                errors.Add("Path must be an absolute path.");
            else if (Path.GetDirectoryName(model.SharedDacpacRepositoryPath)
                  != model.SharedDacpacRepositoryPath!.Substring(0, model.SharedDacpacRepositoryPath.Length - 1))
                errors.Add("Path must be a directory.");
        }
        catch
        {
            errors.Add("Path contains invalid characters.");
        }

        return errors;
    }

    /// <summary>
    ///     Validates the <paramref name="model" />.<see cref="ConfigurationModel.CommentOutUnnamedDefaultConstraintDrops" />
    ///     against the
    ///     <paramref name="model" />.<see cref="ConfigurationModel.ReplaceUnnamedDefaultConstraintDrops" /> and returns all
    ///     found errors.
    /// </summary>
    /// <param name="model">The <see cref="ConfigurationModel" /> instance to validate.</param>
    /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
    public static List<string> ValidateUnnamedDefaultConstraintDropsBehavior(ConfigurationModel model)
    {
        var errors = new List<string>();

        if (model.CommentOutUnnamedDefaultConstraintDrops && model.ReplaceUnnamedDefaultConstraintDrops)
            errors.Add("Behavior for unnamed default constraint drops is ambiguous.");

        return errors;
    }

    /// <summary>
    ///     Validates the <paramref name="model" />.<see cref="ConfigurationModel.VersionPattern" /> and returns all found
    ///     errors.
    /// </summary>
    /// <param name="model">The <see cref="ConfigurationModel" /> instance to validate.</param>
    /// <returns>A list of all found errors. Empty list if no errors were found.</returns>
    public static List<string> ValidateVersionPattern(ConfigurationModel model)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(model.VersionPattern))
        {
            errors.Add("Pattern cannot be empty.");
        }
        else
        {
            var split = model.VersionPattern!.Split(new[] {'.'}, StringSplitOptions.None);
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
            ValidateVersionNumberDigits(position, errors, number);
        else
            ValidateVersionNumberSpecialKeywords(split, position, errors);
    }

    private static void ValidateVersionNumberDigits(int position,
                                                    ICollection<string> errors,
                                                    int number)
    {
        var isNegative = number < 0;
        switch (position)
        {
            case 0 when isNegative:
                errors.Add("Major number cannot be negative.");
                break;
            case 1 when isNegative:
                errors.Add("Minor number cannot be negative.");
                break;
            case 2 when isNegative:
                errors.Add("Build number cannot be negative.");
                break;
            case 3 when isNegative:
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
        var positionValue = split[position];
        switch (position)
        {
            case 0 when positionValue != ConfigurationModel.MajorVersionSpecialKeyword:
                errors.Add("Invalid special keyword for major number.");
                break;
            case 1 when positionValue != ConfigurationModel.MinorVersionSpecialKeyword:
                errors.Add("Invalid special keyword for minor number.");
                break;
            case 2 when positionValue != ConfigurationModel.BuildVersionSpecialKeyword:
                errors.Add("Invalid special keyword for build number.");
                break;
            case 3 when positionValue != ConfigurationModel.RevisionVersionSpecialKeyword:
                errors.Add("Invalid special keyword for revision number.");
                break;
            default:
                return;
        }
    }
}