namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class CreateDeployFilesResultTests
{
    [Test]
    public void Constructor_Errors_CorrectInitialization()
    {
        // Arrange
        var errors = new string[1];

        // Act
        var result = new CreateDeployFilesResult(errors);

        // Assert
        result.DeployScriptContent.Should().BeNull();
        result.DeployReportContent.Should().BeNull();
        result.PreDeploymentScript.Should().BeNull();
        result.PostDeploymentScript.Should().BeNull();
        result.UsedPublishProfile.Should().BeNull();
        result.Errors.Should().BeSameAs(errors);
    }

    [Test]
    public void Constructor_Contents_CorrectInitialization()
    {
        // Arrange
        const string script = "script";
        const string report = "report";
        const string pre = "pre";
        const string post = "post";
        var publishProfile = new PublishProfile();

        // Act
        var result = new CreateDeployFilesResult(script, report, pre, post, publishProfile);

        // Assert
        result.DeployScriptContent.Should().Be(script);
        result.DeployReportContent.Should().Be(report);
        result.PreDeploymentScript.Should().Be(pre);
        result.PostDeploymentScript.Should().Be(post);
        result.UsedPublishProfile.Should().BeSameAs(publishProfile);
        result.Errors.Should().BeNull();
    }
}
