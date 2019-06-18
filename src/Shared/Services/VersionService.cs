namespace SSDTLifecycleExtension.Shared.Services
{
    using System;
    using System.Text;
    using Contracts.Services;
    using JetBrains.Annotations;
    using Models;

    [UsedImplicitly]
    public class VersionService : IVersionService
    {
        string IVersionService.FormatVersion(Version version,
                                             ConfigurationModel configuration)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var configurationVersionPattern = configuration.VersionPattern ?? string.Empty;
            var pattern = configurationVersionPattern.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            if (pattern.Length < 2)
                throw new InvalidOperationException("Version pattern is not long enough. Pattern must at least contain a major and minor number.");
            if (pattern.Length > 4)
                throw new InvalidOperationException("Version pattern is too long. Patter must not contain more than four parts.");

            var final = new StringBuilder();

            for (var i = 0; i < pattern.Length; i++)
            {
                var positionValue = pattern[i];
                switch (positionValue)
                {
                    case ConfigurationModel.MajorVersionSpecialKeyword:
                        final.Append(version.Major);
                        break;
                    case ConfigurationModel.MinorVersionSpecialKeyword:
                        final.Append(version.Minor);
                        break;
                    case ConfigurationModel.BuildVersionSpecialKeyword:
                        final.Append(version.Build);
                        break;
                    case ConfigurationModel.RevisionVersionSpecialKeyword:
                        final.Append(version.Revision);
                        break;
                    default:
                        final.Append(positionValue);
                        break;
                }

                // Add . between each version position
                if (i != pattern.Length - 1)
                    final.Append(".");
            }

            return final.ToString();
        }
    }
}