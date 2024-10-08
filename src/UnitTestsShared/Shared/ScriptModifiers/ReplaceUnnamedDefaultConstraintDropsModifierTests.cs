﻿namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers;

[TestFixture]
public class ReplaceUnnamedDefaultConstraintDropsModifierTests
{
    private const string MultipleDropDefaultConstraintStatements =
        @"PRINT 'Dropping unnamed DEFAULT constraint on dbo.Author ...';
--
GO
ALTER TABLE [dbo].[Author] DROP CONSTRAINT ;

GO
PRINT 'Dropping unnamed DEFAULT constraint on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT ;

GO
PRINT 'Dropping DEFAULT constraint DF_RegisteredDate_Today on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT DF_RegisteredDate_Today;

GO
PRINT 'Update complete'

GO";

    private const string MultipleDropDefaultConstraintStatementsWithPlaceholder =
        @"PRINT 'Dropping unnamed DEFAULT constraint on dbo.Author ...';
--
GO
ALTER TABLE [dbo].[Author] DROP CONSTRAINT ;

GO
PRINT 'Dropping unnamed DEFAULT constraint on dbo.Book ...'

GO
ALTER TABLE [{0}].[Book] DROP CONSTRAINT ;

GO
PRINT 'Dropping DEFAULT constraint DF_RegisteredDate_Today on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT DF_RegisteredDate_Today;

GO
PRINT 'Update complete'

GO";

    private const string MultipleDropDefaultConstraintStatementsReplaced =
        @"PRINT 'Dropping unnamed DEFAULT constraint on dbo.Author ...';
--
GO
DECLARE @schema_name sysname
DECLARE @table_name  sysname
DECLARE @column_name sysname
DECLARE @command     nvarchar(1000)

SET @schema_name = N'dbo'
SET @table_name = N'Author'
SET @column_name = N'LastName'

SELECT @command = 'ALTER TABLE [' + @schema_name + '].[' + @table_name + '] DROP CONSTRAINT ' + d.name
 FROM sys.tables t
 JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
 JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = schema_id(@schema_name)
  AND c.name = @column_name

EXECUTE (@command)

GO
PRINT 'Dropping unnamed DEFAULT constraint on dbo.Book ...'

GO
DECLARE @schema_name sysname
DECLARE @table_name  sysname
DECLARE @column_name sysname
DECLARE @command     nvarchar(1000)

SET @schema_name = N'dbo'
SET @table_name = N'Book'
SET @column_name = N'Title'

SELECT @command = 'ALTER TABLE [' + @schema_name + '].[' + @table_name + '] DROP CONSTRAINT ' + d.name
 FROM sys.tables t
 JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
 JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = schema_id(@schema_name)
  AND c.name = @column_name

EXECUTE (@command)

GO
PRINT 'Dropping DEFAULT constraint DF_RegisteredDate_Today on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT DF_RegisteredDate_Today;

GO
PRINT 'Update complete'

GO";

    private const string MultipleDropDefaultConstraintStatementsReplacedPartially =
        @"PRINT 'Dropping unnamed DEFAULT constraint on dbo.Author ...';
--
GO
DECLARE @schema_name sysname
DECLARE @table_name  sysname
DECLARE @column_name sysname
DECLARE @command     nvarchar(1000)

SET @schema_name = N'dbo'
SET @table_name = N'Author'
SET @column_name = N'LastName'

SELECT @command = 'ALTER TABLE [' + @schema_name + '].[' + @table_name + '] DROP CONSTRAINT ' + d.name
 FROM sys.tables t
 JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
 JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = schema_id(@schema_name)
  AND c.name = @column_name

EXECUTE (@command)

GO
PRINT 'Dropping unnamed DEFAULT constraint on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT ;

GO
PRINT 'Dropping DEFAULT constraint DF_RegisteredDate_Today on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT DF_RegisteredDate_Today;

GO
PRINT 'Update complete'

GO";

    private const string MultipleDropDefaultConstraintStatementsReplacedPartiallyWithPlaceholder =
        @"PRINT 'Dropping unnamed DEFAULT constraint on dbo.Author ...';
--
GO
DECLARE @schema_name sysname
DECLARE @table_name  sysname
DECLARE @column_name sysname
DECLARE @command     nvarchar(1000)

SET @schema_name = N'dbo'
SET @table_name = N'Author'
SET @column_name = N'LastName'

SELECT @command = 'ALTER TABLE [' + @schema_name + '].[' + @table_name + '] DROP CONSTRAINT ' + d.name
 FROM sys.tables t
 JOIN sys.default_constraints d ON d.parent_object_id = t.object_id
 JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = d.parent_column_id
WHERE t.name = @table_name
  AND t.schema_id = schema_id(@schema_name)
  AND c.name = @column_name

EXECUTE (@command)

GO
PRINT 'Dropping unnamed DEFAULT constraint on dbo.Book ...'

GO
ALTER TABLE [{0}].[Book] DROP CONSTRAINT ;

GO
PRINT 'Dropping DEFAULT constraint DF_RegisteredDate_Today on dbo.Book ...'

GO
ALTER TABLE [dbo].[Book] DROP CONSTRAINT DF_RegisteredDate_Today;

GO
PRINT 'Update complete'

GO";

    [Test]
    public async Task ModifyAsync_ErrorsGettingDacpacConstraints_Async()
    {
        // Arrange
        var daMock = new Mock<IDacAccess>();
        var loggerMock = new Mock<ILogger>();
        daMock.Setup(m => m.GetDefaultConstraintsAsync("previousDacpacPath"))
              .ReturnsAsync(() => (Array.Empty<DefaultConstraint>(), new []
              {
                  "oldError1",
                  "oldError2"
              }));
        daMock.Setup(m => m.GetDefaultConstraintsAsync("newDacpacPath"))
              .ReturnsAsync(() => (Array.Empty<DefaultConstraint>(), new[]
              {
                  "newError1",
                  "newError2"
              }));
        var project = new SqlProject("a", "b", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "foobar",
            ReplaceUnnamedDefaultConstraintDrops = true,
            CommentOutUnnamedDefaultConstraintDrops = false,
            PublishProfilePath = "Test.publish.xml",
            VersionPattern = "1.2.3.4",
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "awesome header",
            CustomFooter = "lame footer",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = false
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);
        var model = new ScriptModificationModel(MultipleDropDefaultConstraintStatements, project, config, paths, new Version(1, 0, 0), false);

        // Act
        await modifier.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be(MultipleDropDefaultConstraintStatements);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to load the default constraints of the previous DACPAC:"), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("oldError1"), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("oldError2"), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("Failed to load the default constraints of the current DACPAC:"), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("newError1"), Times.Once);
        loggerMock.Verify(m => m.LogErrorAsync("newError2"), Times.Once);
    }

    [Test]
    public async Task ModifyAsync_NotEnoughStatementsToRemove_Async()
    {
        // Arrange
        var daMock = new Mock<IDacAccess>();
        var loggerMock = new Mock<ILogger>();
        daMock.Setup(m => m.GetDefaultConstraintsAsync("previousDacpacPath"))
              .ReturnsAsync(() => (new[]
              {
                  new DefaultConstraint("dbo", "Author", "LastName", null),
                  new DefaultConstraint("dbo", "Book", "RegisteredDate", "DF_RegisteredDate_Today")
              }, null));
        daMock.Setup(m => m.GetDefaultConstraintsAsync("newDacpacPath"))
              .ReturnsAsync(() => (Array.Empty<DefaultConstraint>(), null));
        var project = new SqlProject("a", "b", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "foobar",
            ReplaceUnnamedDefaultConstraintDrops = true,
            CommentOutUnnamedDefaultConstraintDrops = false,
            PublishProfilePath = "Test.publish.xml",
            VersionPattern = "1.2.3.4",
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "awesome header",
            CustomFooter = "lame footer",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = false
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);
        var model = new ScriptModificationModel(MultipleDropDefaultConstraintStatements, project, config, paths, new Version(1, 0, 0), false);

        // Act
        await modifier.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be(MultipleDropDefaultConstraintStatementsReplacedPartially);
        loggerMock.Verify(m => m.LogWarningAsync($"{nameof(ReplaceUnnamedDefaultConstraintDropsModifier)}: Script defines 1 unnamed default constraint(s) more to drop than the DACPAC models provide."), Times.Once);
    }

    [Test]
    public async Task ModifyAsync_CorrectReplacement_OneRegexTimeout_Async()
    {
        // Arrange
        var schemaName = new string('a', 1_000_000) + '.';
        var input = string.Format(MultipleDropDefaultConstraintStatementsWithPlaceholder, schemaName);
        var expectedOutput = string.Format(MultipleDropDefaultConstraintStatementsReplacedPartiallyWithPlaceholder, schemaName);
        var daMock = new Mock<IDacAccess>();
        var loggerMock = new Mock<ILogger>();
        daMock.Setup(m => m.GetDefaultConstraintsAsync("previousDacpacPath"))
              .ReturnsAsync(() => (new[]
              {
                  new DefaultConstraint("dbo", "Author", "LastName", null),
                  new DefaultConstraint("dbo", "Book", "Title", null),
                  new DefaultConstraint("dbo", "Book", "RegisteredDate", "DF_RegisteredDate_Today")
              }, null));
        daMock.Setup(m => m.GetDefaultConstraintsAsync("newDacpacPath"))
              .ReturnsAsync(() => (Array.Empty<DefaultConstraint>(), null));
        var project = new SqlProject("a", "b", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "foobar",
            ReplaceUnnamedDefaultConstraintDrops = true,
            CommentOutUnnamedDefaultConstraintDrops = false,
            PublishProfilePath = "Test.publish.xml",
            VersionPattern = "1.2.3.4",
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "awesome header",
            CustomFooter = "lame footer",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = false
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);
        var model = new ScriptModificationModel(input, project, config, paths, new Version(1, 0, 0), false);

        // Act
        await modifier.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be(expectedOutput);
        loggerMock.Verify(m => m.LogWarningAsync($"{nameof(ReplaceUnnamedDefaultConstraintDropsModifier)}: Regular expression matching timed out 1 time(s)."), Times.Once);
    }

    [Test]
    public async Task ModifyAsync_CorrectReplacement_Async()
    {
        // Arrange
        var daMock = new Mock<IDacAccess>();
        var loggerMock = new Mock<ILogger>();
        daMock.Setup(m => m.GetDefaultConstraintsAsync("previousDacpacPath"))
              .ReturnsAsync(() => (new[]
              {
                  new DefaultConstraint("dbo", "Author", "LastName", null),
                  new DefaultConstraint("dbo", "Book", "Title", null),
                  new DefaultConstraint("dbo", "Book", "RegisteredDate", "DF_RegisteredDate_Today")
              }, null));
        daMock.Setup(m => m.GetDefaultConstraintsAsync("newDacpacPath"))
              .ReturnsAsync(() => (Array.Empty<DefaultConstraint>(), null));
        var project = new SqlProject("a", "b", "c");
        var config = new ConfigurationModel
        {
            ArtifactsPath = "foobar",
            ReplaceUnnamedDefaultConstraintDrops = true,
            CommentOutUnnamedDefaultConstraintDrops = false,
            PublishProfilePath = "Test.publish.xml",
            VersionPattern = "1.2.3.4",
            CreateDocumentationWithScriptCreation = true,
            CustomHeader = "awesome header",
            CustomFooter = "lame footer",
            BuildBeforeScriptCreation = true,
            TrackDacpacVersion = false
        };
        var directories = new DirectoryPaths("projectDirectory", "latestArtifactsDirectory", "newArtifactsDirectory");
        var sourcePaths = new DeploySourcePaths("newDacpacPath", "publishProfilePath", "previousDacpacPath");
        var targetPaths = new DeployTargetPaths("deployScriptPath", "deployReportPath");
        var paths = new PathCollection(directories, sourcePaths, targetPaths);
        IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);
        var model = new ScriptModificationModel(MultipleDropDefaultConstraintStatements, project, config, paths, new Version(1, 0, 0), false);

        // Act
        await modifier.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be(MultipleDropDefaultConstraintStatementsReplaced);
    }
}