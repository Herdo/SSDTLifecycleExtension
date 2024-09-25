namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class DeploySourcePathsTests
{
    [Test]
    public void Constructor_CorrectSettingOfProperties()
    {
        // Arrange
        const string newDacpacPath = "newDacpacPath";
        const string publishProfilePath = "publishProfilePath";
        const string previousDacpacPath = "previousDacpacPath";

        // Act
        var dsp = new DeploySourcePaths(newDacpacPath, publishProfilePath, previousDacpacPath);

        // Assert
        dsp.NewDacpacPath.Should().Be(newDacpacPath);
        dsp.PublishProfilePath.Should().Be(publishProfilePath);
        dsp.PreviousDacpacPath.Should().Be(previousDacpacPath);
    }
}