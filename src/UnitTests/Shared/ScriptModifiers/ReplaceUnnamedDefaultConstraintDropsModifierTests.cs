using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using System.Threading.Tasks;
    using Moq;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Contracts.DataAccess;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

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
  AND c.name = column_name

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
  AND c.name = column_name

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
  AND c.name = column_name

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
  AND c.name = column_name

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
        public void Constructor_ArgumentNullException_DacAccess()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ReplaceUnnamedDefaultConstraintDropsModifier(null, null));
        }

        [Test]
        public void Constructor_ArgumentNullException_Logger()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new ReplaceUnnamedDefaultConstraintDropsModifier(daMock, null));
        }

        [Test]
        public void ModifyAsync_ArgumentNullException_Input()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var loggerMock = Mock.Of<ILogger>();
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock, loggerMock);
            
            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void ModifyAsync_ArgumentNullException_Project()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var input = "";
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(input, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void ModifyAsync_ArgumentNullException_Configuration()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var input = "";
            var project = new SqlProject("a", "b", "c");
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(input, project, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void ModifyAsync_ArgumentNullException_Paths()
        {
            // Arrange
            var daMock = Mock.Of<IDacAccess>();
            var loggerMock = Mock.Of<ILogger>();
            var input = "";
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
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock, loggerMock);

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(input, project, config, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task ModifyAsync_ErrorsGettingDacpacConstraints_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            var loggerMock = new Mock<ILogger>();
            daMock.Setup(m => m.GetDefaultConstraintsAsync("old"))
                  .ReturnsAsync(() => (new DefaultConstraint[0], new []
                                          {
                                              "oldError1",
                                              "oldError2"
                                          }));
            daMock.Setup(m => m.GetDefaultConstraintsAsync("new"))
                  .ReturnsAsync(() => (new DefaultConstraint[0], new[]
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
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var paths = new PathCollection("a", "b", "new", "old", "e", "f");
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);

            // Act
            var result = await modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, config, paths);

            // Assert
            Assert.AreEqual(MultipleDropDefaultConstraintStatements, result);
            loggerMock.Verify(m => m.LogAsync("ERROR: Failed to load the default constraints of the previous DACPAC:"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("oldError1"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("oldError2"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("ERROR: Failed to load the default constraints of the current DACPAC:"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("newError1"), Times.Once);
            loggerMock.Verify(m => m.LogAsync("newError2"), Times.Once);
        }

        [Test]
        public async Task ModifyAsync_NotEnoughStatementsToRemove_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            var loggerMock = new Mock<ILogger>();
            daMock.Setup(m => m.GetDefaultConstraintsAsync("old"))
                  .ReturnsAsync(() => (new[]
                                          {
                                              new DefaultConstraint("dbo", "Author", "LastName", null),
                                              new DefaultConstraint("dbo", "Book", "RegisteredDate", "DF_RegisteredDate_Today")
                                          }, null));
            daMock.Setup(m => m.GetDefaultConstraintsAsync("new"))
                  .ReturnsAsync(() => (new DefaultConstraint[0], null));
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
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var paths = new PathCollection("a", "b", "new", "old", "e", "f");
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);

            // Act
            var result = await modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, config, paths);

            // Assert
            Assert.AreEqual(MultipleDropDefaultConstraintStatementsReplacedPartially, result);
            loggerMock.Verify(m => m.LogAsync($"WARNING - {nameof(ReplaceUnnamedDefaultConstraintDropsModifier)}: Script defines 1 unnamed default constraint(s) more to drop than the DACPAC models provide."), Times.Once);
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
            daMock.Setup(m => m.GetDefaultConstraintsAsync("old"))
                  .ReturnsAsync(() => (new[]
                                          {
                                              new DefaultConstraint("dbo", "Author", "LastName", null),
                                              new DefaultConstraint("dbo", "Book", "Title", null),
                                              new DefaultConstraint("dbo", "Book", "RegisteredDate", "DF_RegisteredDate_Today")
                                          }, null));
            daMock.Setup(m => m.GetDefaultConstraintsAsync("new"))
                  .ReturnsAsync(() => (new DefaultConstraint[0], null));
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
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var paths = new PathCollection("a", "b", "new", "old", "e", "f");
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);

            // Act
            var result = await modifier.ModifyAsync(input, project, config, paths);

            // Assert
            Assert.AreEqual(expectedOutput, result);
            loggerMock.Verify(m => m.LogAsync($"WARNING - {nameof(ReplaceUnnamedDefaultConstraintDropsModifier)}: Regular expression matching timed out 1 time(s)."), Times.Once);
        }

        [Test]
        public async Task ModifyAsync_CorrectReplacement_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            var loggerMock = new Mock<ILogger>();
            daMock.Setup(m => m.GetDefaultConstraintsAsync("old"))
                  .ReturnsAsync(() => (new[]
                                          {
                                              new DefaultConstraint("dbo", "Author", "LastName", null),
                                              new DefaultConstraint("dbo", "Book", "Title", null),
                                              new DefaultConstraint("dbo", "Book", "RegisteredDate", "DF_RegisteredDate_Today")
                                          }, null));
            daMock.Setup(m => m.GetDefaultConstraintsAsync("new"))
                  .ReturnsAsync(() => (new DefaultConstraint[0], null));
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
                TrackDacpacVersion = false,
                CommentOutReferencedProjectRefactorings = true
            };
            var paths = new PathCollection("a", "b", "new", "old", "e", "f");
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);

            // Act
            var result = await modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, config, paths);

            // Assert
            Assert.AreEqual(MultipleDropDefaultConstraintStatementsReplaced, result);
        }
    }
}