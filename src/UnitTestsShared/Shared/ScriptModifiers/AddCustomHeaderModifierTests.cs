namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers;

[TestFixture]
public class AddCustomHeaderModifierTests
{
    [Test]
    public async Task Modify_CustomHeaderAdded_Async()
    {
        // Arrange
        IScriptModifier s = new AddCustomHeaderModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = new Version(1, 1, 0);
        var configuration = new ConfigurationModel
        {
            CustomHeader = "Header"
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 0, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("Header\r\nfoobar");
    }

    [Test]
    [TestCase(null,TestName = "<null>")]
    [TestCase("", TestName = "<empty string>")]
    [TestCase("    ", TestName = "<4 whitespaces>")]
    [TestCase(" ", TestName = "<1 tabulator>")]
    public async Task Modify_NoLeadingNewLineWhenNullOrWhiteSpace_Async(string customHeader)
    {
        // Arrange
        IScriptModifier s = new AddCustomHeaderModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        var configuration = new ConfigurationModel
        {
            CustomHeader = customHeader
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 0, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("foobar");
    }

    [Test]
    public async Task Modify_ReplaceSpecialKeyword_PreviousVersion_Async()
    {
        // Arrange
        IScriptModifier s = new AddCustomHeaderModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = new Version(1, 3 , 0);
        var configuration = new ConfigurationModel
        {
            CustomHeader = "Script base version: {PREVIOUS_VERSION}"
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("Script base version: 1.2.0\r\nfoobar");
    }

    [Test]
    public async Task Modify_ReplaceSpecialKeyword_NextVersion_Async()
    {
        // Arrange
        IScriptModifier s = new AddCustomHeaderModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = new Version(1, 3, 0);
        var configuration = new ConfigurationModel
        {
            CustomHeader = "Script target version: {NEXT_VERSION}"
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("Script target version: 1.3.0\r\nfoobar");
    }
}