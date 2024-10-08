﻿namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers;

[TestFixture]
public class AddCustomFooterModifierTests
{
    [Test]
    public async Task Modify_CustomFooterAdded_Async()
    {
        // Arrange
        IScriptModifier s = new AddCustomFooterModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = new Version(1, 3, 0);
        var configuration = new ConfigurationModel
        {
            CustomFooter = "footer"
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("foobar\r\nfooter");
    }

    [Test]
    [TestCase(null,TestName = "<null>")]
    [TestCase("", TestName = "<empty string>")]
    [TestCase("    ", TestName = "<4 whitespaces>")]
    [TestCase(" ", TestName = "<1 tabulator>")]
    public async Task Modify_NoTrailingNewLineWhenNullOrWhiteSpace_Async(string customFooter)
    {
        // Arrange
        IScriptModifier s = new AddCustomFooterModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        var configuration = new ConfigurationModel
        {
            CustomFooter = customFooter
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
        IScriptModifier s = new AddCustomFooterModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = new Version(1, 3, 0);
        var configuration = new ConfigurationModel
        {
            CustomFooter = "Script base version: {PREVIOUS_VERSION}"
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("foobar\r\nScript base version: 1.2.0");
    }

    [Test]
    public async Task Modify_ReplaceSpecialKeyword_NextVersion_Async()
    {
        // Arrange
        IScriptModifier s = new AddCustomFooterModifier();
        const string input = "foobar";
        var project = new SqlProject("a", "b", "c");
        project.ProjectProperties.DacVersion = new Version(1, 3, 0);
        var configuration = new ConfigurationModel
        {
            CustomFooter = "Script target version: {NEXT_VERSION}"
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        var model = new ScriptModificationModel(input, project, configuration, paths, new Version(1, 2, 0), false);

        // Act
        await s.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be("foobar\r\nScript target version: 1.3.0");
    }
}