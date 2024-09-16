namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class VersionServiceTests
{
    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("1")]
    [TestCase("100")]
    [TestCase(ConfigurationModel.MajorVersionSpecialKeyword)]
    public void FormatVersion_InvalidOperationException_PatternNotLongEnough(string configuredPattern)
    {
        // Arrange
        IVersionService vs = new VersionService();
        var v = new Version(1, 2, 3, 4);
        var cm = new ConfigurationModel {VersionPattern = configuredPattern};

        // Act
        var e = Assert.Throws<InvalidOperationException>(() => vs.FormatVersion(v, cm));

        // Assert
        e.Message.Should().Contain("not long enough");
    }

    [Test]
    [TestCase("100.200.300.400.500.600.700")]
    [TestCase("1.2.3.4.5")]
    [TestCase(ConfigurationModel.MajorVersionSpecialKeyword + "." + ConfigurationModel.MinorVersionSpecialKeyword + "." + ConfigurationModel.BuildVersionSpecialKeyword + "." + ConfigurationModel.RevisionVersionSpecialKeyword + ".1")]
    public void FormatVersion_InvalidOperationException_PatternTooLong(string configuredPattern)
    {
        // Arrange
        IVersionService vs = new VersionService();
        var v = new Version(1, 2, 3, 4);
        var cm = new ConfigurationModel { VersionPattern = configuredPattern };

        // Act
        var e = Assert.Throws<InvalidOperationException>(() => vs.FormatVersion(v, cm));

        // Assert
        e.Message.Should().Contain("too long");
    }

    [Test]
    [TestCase("0.0", "0.0")]
    [TestCase(ConfigurationModel.MajorVersionSpecialKeyword + ".0", "1.0")]
    [TestCase(ConfigurationModel.MajorVersionSpecialKeyword + "." + ConfigurationModel.MinorVersionSpecialKeyword + ".0", "1.2.0")]
    [TestCase(ConfigurationModel.MajorVersionSpecialKeyword + "." + ConfigurationModel.MinorVersionSpecialKeyword + "." + ConfigurationModel.BuildVersionSpecialKeyword + ".0", "1.2.3.0")]
    [TestCase("4.3.2.1", "4.3.2.1")]
    [TestCase(ConfigurationModel.MajorVersionSpecialKeyword + "." + ConfigurationModel.MinorVersionSpecialKeyword + "." + ConfigurationModel.BuildVersionSpecialKeyword + "." + ConfigurationModel.RevisionVersionSpecialKeyword, "1.2.3.4")]
    public void FormatVersion_CorrectFormat(string configuredPattern, string expectedResult)
    {
        // Arrange
        IVersionService vs = new VersionService();
        var v = new Version(1, 2, 3, 4);
        var cm = new ConfigurationModel { VersionPattern = configuredPattern };

        // Act
        var formatted = vs.FormatVersion(v, cm);

        // Assert
        formatted.Should().Be(expectedResult);
    }
}