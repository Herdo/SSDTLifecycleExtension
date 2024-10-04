namespace SSDTLifecycleExtension.UnitTests.Shared.ModelValidations;

[TestFixture]
public class ConfigurationModelValidationsTests
{
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Path cannot be empty.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Path contains invalid characters.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Path must be a relative path.");
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
        errors.Should().BeEmpty();
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Path cannot be empty.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Profile file name must end with *.publish.xml.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Path contains invalid characters.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Path must be a relative path.");
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
        errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateSharedDacpacRepositoryPath_Errors_InvalidCharacters()
    {
        // Arrange
        var model = new ConfigurationModel
        {
            SharedDacpacRepositoryPaths = "C:\\" + new string(Path.GetInvalidPathChars()) + "\\Test\\"
        };

        // Act
        var errors = ConfigurationModelValidations.ValidateSharedDacpacRepositoryPaths(model);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Contain("is invalid");
    }

    [Test]
    [TestCase(@"C:\Temp\.")]
    [TestCase(@"C:\Temp\test.foo")]
    [TestCase(@"C:\Temp\.foo")]
    public void ValidateSharedDacpacRepositoryPath_Errors_MustBeDirectory(string path)
    {
        // Arrange
        var model = new ConfigurationModel
        {
            SharedDacpacRepositoryPaths = path
        };

        // Act
        var errors = ConfigurationModelValidations.ValidateSharedDacpacRepositoryPaths(model);

        // Assert
        errors.Should().ContainSingle()
            .Which.Should().Be($"Path '{path}' must be a directory.");
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public void ValidateSharedDacpacRepositoryPath_NoErrors_EmptyPath(string path)
    {
        // Arrange
        var model = new ConfigurationModel
        {
            SharedDacpacRepositoryPaths = path
        };

        // Act
        var errors = ConfigurationModelValidations.ValidateSharedDacpacRepositoryPaths(model);

        // Assert
        errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateSharedDacpacRepositoryPaths_NoErrors()
    {
        // Arrange
        var model = new ConfigurationModel
        {
            SharedDacpacRepositoryPaths = @"C:\Test\Repository\;.\_Deployment\;..\Repository\;_Current\;\foo\"
        };

        // Act
        var errors = ConfigurationModelValidations.ValidateSharedDacpacRepositoryPaths(model);

        // Assert
        errors.Should().BeEmpty();
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Behavior for unnamed default constraint drops is ambiguous.");
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
        errors.Should().BeEmpty();
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Pattern cannot be empty.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Pattern doesn't contain enough parts.");
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
        errors.Should().ContainSingle()
            .Which.Should().Be("Pattern contains too many parts.");
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
        errors.Should().HaveCount(4);
        errors[0].Should().Be("Major number cannot be negative.");
        errors[1].Should().Be("Minor number cannot be negative.");
        errors[2].Should().Be("Build number cannot be negative.");
        errors[3].Should().Be("Revision number cannot be negative.");
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
        errors.Should().HaveCount(4);
        errors[0].Should().Be("Invalid special keyword for major number.");
        errors[1].Should().Be("Invalid special keyword for minor number.");
        errors[2].Should().Be("Invalid special keyword for build number.");
        errors[3].Should().Be("Invalid special keyword for revision number.");
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
        errors.Should().BeEmpty();
    }
}