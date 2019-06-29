using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ModelValidations
{
    using System;
    using System.IO;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ModelValidations;

    [TestFixture]
    public class ConfigurationModelValidationsTests
    {
        [Test]
        public void ValidateArtifactsPath_ArgumentNullException_Model()
        {
            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ConfigurationModelValidations.ValidateArtifactsPath(null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void ValidateArtifactsPath_Errors_EmptyPath(string path)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = path
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateArtifactsPath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Path cannot be empty.", errors[0]);
        }

        [Test]
        public void ValidateArtifactsPath_Errors_InvalidCharacters()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = new string(Path.GetInvalidPathChars())
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateArtifactsPath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Path contains invalid characters.", errors[0]);
        }

        [Test]
        public void ValidateArtifactsPath_Errors_NoRelativePath()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = @"C:\Temp\_Deployment"
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateArtifactsPath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Path must be a relative path.", errors[0]);
        }

        [Test]
        public void ValidateArtifactsPath_NoErrors()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                ArtifactsPath = @"..\_Deployment"
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateArtifactsPath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void ValidatePublishProfilePath_ArgumentNullException_Model()
        {
            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ConfigurationModelValidations.ValidatePublishProfilePath(null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void ValidatePublishProfilePath_Errors_EmptyPath(string path)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                PublishProfilePath = path
            };

            // Act
            var errors = ConfigurationModelValidations.ValidatePublishProfilePath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Path cannot be empty.", errors[0]);
        }

        [Test]
        [TestCase(".publish.xml")] // Just the ending is not OK.
        [TestCase("Database.pub.xml")] // Wrong ending is not OK.
        public void ValidatePublishProfilePath_Errors_DoesNotEndCorrectly(string path)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                PublishProfilePath = path
            };

            // Act
            var errors = ConfigurationModelValidations.ValidatePublishProfilePath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Profile file name must end with *.publish.xml.", errors[0]);
        }

        [Test]
        public void ValidatePublishProfilePath_Errors_InvalidCharacters()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                PublishProfilePath = new string(Path.GetInvalidPathChars()) + ".publish.xml"
            };

            // Act
            var errors = ConfigurationModelValidations.ValidatePublishProfilePath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Path contains invalid characters.", errors[0]);
        }

        [Test]
        public void ValidatePublishProfilePath_Errors_NoRelativePath()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                PublishProfilePath = @"C:\Temp\Database.publish.xml"
            };

            // Act
            var errors = ConfigurationModelValidations.ValidatePublishProfilePath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Path must be a relative path.", errors[0]);
        }

        [Test]
        [TestCase(@"..\Database.publish.xml")]
        [TestCase(ConfigurationModel.UseSinglePublishProfileSpecialKeyword)]
        public void ValidatePublishProfilePath_NoErrors(string publishProfilePath)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                PublishProfilePath = publishProfilePath
            };

            // Act
            var errors = ConfigurationModelValidations.ValidatePublishProfilePath(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void ValidateUnnamedDefaultConstraintDropsBehavior_ArgumentNullException_Model()
        {
            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ConfigurationModelValidations.ValidateUnnamedDefaultConstraintDropsBehavior(null));
        }

        [Test]
        public void ValidateUnnamedDefaultConstraintDropsBehavior_Errors_BothSetToTrue()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                CommentOutUnnamedDefaultConstraintDrops = true,
                ReplaceUnnamedDefaultConstraintDrops = true
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateUnnamedDefaultConstraintDropsBehavior(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Behavior for unnamed default constraint drops is ambiguous.", errors[0]);
        }

        [Test]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void ValidateUnnamedDefaultConstraintDropsBehavior_NoErrors(bool commentOut, bool replace)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                CommentOutUnnamedDefaultConstraintDrops = commentOut,
                ReplaceUnnamedDefaultConstraintDrops = replace
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateUnnamedDefaultConstraintDropsBehavior(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void ValidateVersionPattern_ArgumentNullException_Model()
        {
            // Act & Assert
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => ConfigurationModelValidations.ValidateVersionPattern(null));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("    ")]
        public void ValidateVersionPattern_Errors_EmptyPattern(string pattern)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                VersionPattern = pattern
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateVersionPattern(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Pattern cannot be empty.", errors[0]);
        }

        [Test]
        [TestCase("1")]
        [TestCase("{MAJOR}")]
        public void ValidateVersionPattern_Errors_PatternTooShort(string pattern)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                VersionPattern = pattern
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateVersionPattern(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Pattern doesn't contain enough parts.", errors[0]);
        }

        [Test]
        [TestCase("1.2.3.4.5")]
        [TestCase("{MAJOR}.{MINOR}.{BUILD}.{REVISION}.0")]
        public void ValidateVersionPattern_Errors_PatternTooLong(string pattern)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                VersionPattern = pattern
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateVersionPattern(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Pattern contains too many parts.", errors[0]);
        }

        [Test]
        public void ValidateVersionPattern_Errors_NegativeNumbers()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                VersionPattern = "-1.-1.-1.-1"
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateVersionPattern(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(4, errors.Count);
            Assert.AreEqual("Major number cannot be negative.", errors[0]);
            Assert.AreEqual("Minor number cannot be negative.", errors[1]);
            Assert.AreEqual("Build number cannot be negative.", errors[2]);
            Assert.AreEqual("Revision number cannot be negative.", errors[3]);
        }

        [Test]
        public void ValidateVersionPattern_Errors_InvalidSpecialKeywords()
        {
            // Arrange
            var model = new ConfigurationModel
            {
                VersionPattern = "{REVISION}.{BUILD}.{MINOR}.{MAJOR}"
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateVersionPattern(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(4, errors.Count);
            Assert.AreEqual("Invalid special keyword for major number.", errors[0]);
            Assert.AreEqual("Invalid special keyword for minor number.", errors[1]);
            Assert.AreEqual("Invalid special keyword for build number.", errors[2]);
            Assert.AreEqual("Invalid special keyword for revision number.", errors[3]);
        }

        [Test]
        [TestCase("1.2.3.4")]
        [TestCase("{MAJOR}.{MINOR}.{BUILD}.{REVISION}")]
        [TestCase("{MAJOR}.1.{BUILD}.{REVISION}")]
        [TestCase("{MAJOR}.1.{BUILD}.0")]
        [TestCase("1.0.{BUILD}.0")]
        public void ValidateVersionPattern_NoErrors(string pattern)
        {
            // Arrange
            var model = new ConfigurationModel
            {
                VersionPattern = pattern
            };

            // Act
            var errors = ConfigurationModelValidations.ValidateVersionPattern(model);

            // Assert
            Assert.IsNotNull(errors);
            Assert.AreEqual(0, errors.Count);
        }
    }
}