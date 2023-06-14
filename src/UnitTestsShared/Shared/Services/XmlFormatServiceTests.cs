namespace SSDTLifecycleExtension.UnitTests.Shared.Services;

[TestFixture]
public class XmlFormatServiceTests
{
    [Test]
    public void FormatDeployReport_NullInNullOut()
    {
        // Arrange
        IXmlFormatService service = new XmlFormatService();

        // Act
        var result = service.FormatDeployReport(null);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public void FormatDeployReport_XmlException_InvalidXml()
    {
        // Arrange
        const string invalidXml = "<?xml version=\"1.0\" encoding=\"utf-8";
        IXmlFormatService service = new XmlFormatService();

        // Act & Assert
        Assert.Throws<XmlException>(() => service.FormatDeployReport(invalidXml));
    }

    [Test]
    public void FormatDeployReport_CorrectFormat()
    {
        // Arrange
        const string singleLine = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DeploymentReport xmlns=\"http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02\"><Alerts /><Operations><Operation Name=\"Create\"><Item Value=\"DEFAULT-Constraint: unnamed constraint on [dbo].[Author]\" Type=\"SqlDefaultConstraint\" /></Operation></Operations></DeploymentReport>";
        const string multiLine =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<DeploymentReport xmlns=""http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02"">
    <Alerts />
    <Operations>
        <Operation Name=""Create"">
            <Item Value=""DEFAULT-Constraint: unnamed constraint on [dbo].[Author]"" Type=""SqlDefaultConstraint"" />
        </Operation>
    </Operations>
</DeploymentReport>";
        IXmlFormatService service = new XmlFormatService();

        // Act
        var result = service.FormatDeployReport(singleLine);

        // Assert
        Assert.AreEqual(multiLine, result);
    }
}