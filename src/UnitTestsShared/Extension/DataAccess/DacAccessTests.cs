namespace SSDTLifecycleExtension.UnitTests.Extension.DataAccess;

[TestFixture]
[SetUICulture("en-US")]
public class DacAccessTests
{
    private static readonly List<string> CreatedFiles;

    static DacAccessTests()
    {
        CreatedFiles = new List<string>();
    }

    /// <summary>
    /// Writes the embedded resource associated with the <paramref name="resourceName"/> to a temporary file.
    /// </summary>
    /// <param name="resourceName">The name of the resource to write to a temporary file.</param>
    /// <returns>The full path of the temporary file.</returns>
    private static string WriteEmbeddedResourceToTemporaryFile(string resourceName)
    {
        var tempFileName = Path.GetTempPath() + Guid.NewGuid().ToString("N") + ".dacpac";
        CreatedFiles.Add(tempFileName);
        using var sourceStream = GetEmbeddedResourceStream(resourceName);
        using var targetStream = new FileStream(tempFileName, FileMode.Create);
        sourceStream.CopyTo(targetStream);
        targetStream.Flush();

        return tempFileName;
    }

    private static Stream GetEmbeddedResourceStream(string resourceName)
    {
        var testAssembly = typeof(DacAccessTests).Assembly;
        var resourceNames = testAssembly.GetManifestResourceNames();
        var fullResourceName = resourceNames.Single(m => m.EndsWith(resourceName));
        return testAssembly.GetManifestResourceStream(fullResourceName);
    }

    [TearDown]
    public static void TearDown()
    {
        foreach (var createdFile in CreatedFiles)
        {
            var retries = 10;
            var deleted = false;
            while (!deleted && retries > 0)
            {
                try
                {
                    File.Delete(createdFile);
                    deleted = true;
                }
                catch
                {
                    retries--;
                    Thread.Sleep(50);
                }
            }
        }
        CreatedFiles.Clear();
    }

    [Test]
    public void CreateDeployFilesAsync_InvalidOperationException_CreateNoOutput()
    {
        // Arrange
        var xfsMock = Mock.Of<IXmlFormatService>();
        IDacAccess da = new DacAccess(xfsMock);
        var previousVersionDacpacPath = "path1";
        var newVersionDacpacPath = "path2";
        var publishProfilePath = "path3";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => da.CreateDeployFilesAsync(previousVersionDacpacPath, newVersionDacpacPath, publishProfilePath, false, false));
    }

    [Test]
    public async Task CreateDeployFilesAsync_CorrectCreation_Async()
    {
        // Arrange
        var xfsMock = new Mock<IXmlFormatService>();
        xfsMock.Setup(m => m.FormatDeployReport(It.IsNotNull<string>())).Returns((string s) => s);
        var tempPreviousVersionDacpacPath = WriteEmbeddedResourceToTemporaryFile("TestDatabase_Empty.dacpac");
        var tempNewVersionDacpacPath = WriteEmbeddedResourceToTemporaryFile("TestDatabase_WithAuthorTable.dacpac");
        var tempPublishProfilePath = WriteEmbeddedResourceToTemporaryFile("TestDatabase.publish.xml");
        IDacAccess da = new DacAccess(xfsMock.Object);

        // Act
        var result = await da.CreateDeployFilesAsync(tempPreviousVersionDacpacPath, tempNewVersionDacpacPath, tempPublishProfilePath, true, true);

        // Assert
        result.Errors.Should().BeNullOrEmpty();
        result.DeployScriptContent.Should().NotBeNull();
        result.DeployReportContent.Should().NotBeNull();
        result.Errors.Should().BeNull();
        xfsMock.Verify(m => m.FormatDeployReport(It.IsNotNull<string>()), Times.Once);
        // Verify script
        var productionIndex = result.DeployScriptContent.IndexOf("PRODUCTION", StringComparison.InvariantCulture);
        productionIndex.Should().BeGreaterThan(0);
        var onErrorIndex = result.DeployScriptContent.IndexOf(":on error exit", StringComparison.InvariantCulture);
        onErrorIndex.Should().BeGreaterThan(productionIndex);
        var changeDatabaseIndex = result.DeployScriptContent.IndexOf("USE [$(DatabaseName)]", StringComparison.InvariantCulture);
        changeDatabaseIndex.Should().BeGreaterThan(onErrorIndex);
        var createAuthorPrintIndex = result.DeployScriptContent.IndexOf("[dbo].[Author]...';", StringComparison.InvariantCulture);
        createAuthorPrintIndex.Should().BeGreaterThan(changeDatabaseIndex);
        var createAuthorTableIndex = result.DeployScriptContent.IndexOf("CREATE TABLE [dbo].[Author]", StringComparison.InvariantCulture);
        createAuthorTableIndex.Should().BeGreaterThan(createAuthorPrintIndex);
        // Verify report
        result.DeployReportContent.Should()
              .Be(
                  @"<?xml version=""1.0"" encoding=""utf-8""?><DeploymentReport xmlns=""http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02""><Alerts /><Operations><Operation Name=""Create""><Item Value=""[dbo].[Author]"" Type=""SqlTable"" /></Operation></Operations></DeploymentReport>");
    }

    [Test]
    public async Task CreateDeployFilesAsync_CorrectCreation_WithPreAndPostDeploymentScripts_Async()
    {
        // Arrange
        var xfsMock = new Mock<IXmlFormatService>();
        xfsMock.Setup(m => m.FormatDeployReport(It.IsNotNull<string>())).Returns((string s) => s);
        var tempPreviousVersionDacpacPath = WriteEmbeddedResourceToTemporaryFile("TestDatabase_Empty.dacpac");
        var tempNewVersionDacpacPath = WriteEmbeddedResourceToTemporaryFile("TestDatabase_WithPreAndPostDeployment.dacpac");
        var tempPublishProfilePath = WriteEmbeddedResourceToTemporaryFile("TestDatabase.publish.xml");
        IDacAccess da = new DacAccess(xfsMock.Object);

        // Act
        var result = await da.CreateDeployFilesAsync(tempPreviousVersionDacpacPath, tempNewVersionDacpacPath, tempPublishProfilePath, true, true);

        // Assert
        result.Errors.Should().BeNullOrEmpty();
        result.DeployScriptContent.Should().NotBeNull();
        result.DeployReportContent.Should().NotBeNull();
        result.PreDeploymentScript.Should().Be("-- Pre-deployment script content goes here\r\nGO\r\n");
        result.PostDeploymentScript.Should().Be("-- Post-deployment script content goes here\r\nGO\r\n");
        result.Errors.Should().BeNull();
        xfsMock.Verify(m => m.FormatDeployReport(It.IsNotNull<string>()), Times.Once);
        // Verify script
        var productionIndex = result.DeployScriptContent.IndexOf("PRODUCTION", StringComparison.InvariantCulture);
        productionIndex.Should().BeGreaterThan(0);
        var onErrorIndex = result.DeployScriptContent.IndexOf(":on error exit", StringComparison.InvariantCulture);
        onErrorIndex.Should().BeGreaterThan(productionIndex);
        var changeDatabaseIndex = result.DeployScriptContent.IndexOf("USE [$(DatabaseName)]", StringComparison.InvariantCulture);
        changeDatabaseIndex.Should().BeGreaterThan(onErrorIndex);
        var preDeploymentIndex = result.DeployScriptContent.IndexOf(result.PreDeploymentScript, StringComparison.InvariantCulture);
        preDeploymentIndex.Should().BeGreaterThan(changeDatabaseIndex);
        var createAuthorPrintIndex = result.DeployScriptContent.IndexOf("[dbo].[Author]...';", StringComparison.InvariantCulture);
        createAuthorPrintIndex.Should().BeGreaterThan(preDeploymentIndex);
        var createAuthorTableIndex = result.DeployScriptContent.IndexOf("CREATE TABLE [dbo].[Author]", StringComparison.InvariantCulture);
        createAuthorTableIndex.Should().BeGreaterThan(createAuthorPrintIndex);
        var postDeploymentIndex = result.DeployScriptContent.IndexOf(result.PostDeploymentScript, StringComparison.InvariantCulture);
        postDeploymentIndex.Should().BeGreaterThan(createAuthorTableIndex);
        // Verify report
        result.DeployReportContent.Should().Be(@"<?xml version=""1.0"" encoding=""utf-8""?><DeploymentReport xmlns=""http://schemas.microsoft.com/sqlserver/dac/DeployReport/2012/02""><Alerts />" +
            @"<Operations><Operation Name=""Create""><Item Value=""[dbo].[Author]"" Type=""SqlTable"" /><Item Value=""[dbo].[DF_Birthday_Today]"" Type=""SqlDefaultConstraint"" /></Operation>" +
            @"</Operations></DeploymentReport>");
    }

    [Test]
    public async Task GetDefaultConstraintsAsync_CorrectCreation_MoreConstraints_Async()
    {
        // Arrange
        var xfsMock = Mock.Of<IXmlFormatService>();
        var tempDacpacPath = WriteEmbeddedResourceToTemporaryFile("TestDatabase_AuthorWithDefaultConstraints.dacpac");
        IDacAccess da = new DacAccess(xfsMock);

        // Act
        var (defaultConstraints, errors) = await da.GetDefaultConstraintsAsync(tempDacpacPath);

        // Assert
        defaultConstraints.Should().NotBeNull();
        errors.Should().BeNull();
        defaultConstraints.Should().HaveCount(3);
        var orderedConstraints = defaultConstraints.OrderBy(m => m.ColumnName)
                                                   .ToArray();
        // First constraint
        orderedConstraints[0].TableSchema.Should().Be("dbo");
        orderedConstraints[0].TableName.Should().Be("Author");
        orderedConstraints[0].ColumnName.Should().Be("Birthday");
        orderedConstraints[0].ConstraintName.Should().Be("DF_Birthday_Today");
        // Second constraint
        orderedConstraints[1].TableSchema.Should().Be("dbo");
        orderedConstraints[1].TableName.Should().Be("Author");
        orderedConstraints[1].ColumnName.Should().Be("FirstName");
        orderedConstraints[1].ConstraintName.Should().Be("DF_FirstName_Empty");
        // Third constraint
        orderedConstraints[2].TableSchema.Should().Be("dbo");
        orderedConstraints[2].TableName.Should().Be("Author");
        orderedConstraints[2].ColumnName.Should().Be("LastName");
        orderedConstraints[2].ConstraintName.Should().BeNull();
    }

    [Test]
    public async Task GetDefaultConstraintsAsync_CorrectCreation_SingleConstraints_Async()
    {
        // Arrange
        var xfsMock = Mock.Of<IXmlFormatService>();
        var tempDacpacPath = WriteEmbeddedResourceToTemporaryFile("TestDatabase_AuthorWithLessDefaultConstraints.dacpac");
        IDacAccess da = new DacAccess(xfsMock);

        // Act
        var (defaultConstraints, errors) = await da.GetDefaultConstraintsAsync(tempDacpacPath);

        // Assert
        defaultConstraints.Should().NotBeNull();
        errors.Should().BeNull();
        var defaultConstraint = defaultConstraints.Should().ContainSingle().Subject;
        defaultConstraint.TableSchema.Should().Be("dbo");
        defaultConstraint.TableName.Should().Be("Author");
        defaultConstraint.ColumnName.Should().Be("Birthday");
        defaultConstraint.ConstraintName.Should().Be("DF_Birthday_Today");
    }
}