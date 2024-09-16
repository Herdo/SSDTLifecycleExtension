namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers;

[TestFixture]
public class RemoveSqlCmdStatementsModifierTests
{
    private const string MultiLineInputWithSqlcmdStatementsWithCommentedOutSetVar =
        @"SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;

GO
/*
:setvar DatabaseName ""PRODUCTION""
:setvar DefaultFilePrefix ""PRODUCTION""
:setvar DefaultDataPath """"
:setvar DefaultLogPath """"
*/

GO
:on error exit
GO
/*
Check SQLCMD mode comment
SET NOEXEC OFF;
*/
:setvar __IsSqlCmdEnabled ""True""
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'Print SQLCMD mode must be enabled';
        SET NOEXEC ON;
    END


GO
USE [$(DatabaseName)];

GO
PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
USE [$(DatabaseName)];

GO
PRINT 'Update complete'

GO";

    private const string MultiLineInputWithSqlcmdStatementsWithoutCommentedOutSetVar =
        @"SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;

GO
:setvar DatabaseName ""PRODUCTION""
:setvar DefaultFilePrefix ""PRODUCTION""
:setvar DefaultDataPath """"
:setvar DefaultLogPath """"

GO
:on error exit
GO
/*
Check SQLCMD mode comment
SET NOEXEC OFF;
*/
:setvar __IsSqlCmdEnabled ""True""
GO
IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'Print SQLCMD mode must be enabled';
        SET NOEXEC ON;
    END


GO
USE [$(DatabaseName)];

GO
PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
USE [$(DatabaseName)];

GO
PRINT 'Update complete'

GO";

    private const string MultiLineInputWithoutSqlcmdStatements =
        @"SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;

GO
PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'

GO";

    [Test]
    [TestCase(MultiLineInputWithSqlcmdStatementsWithCommentedOutSetVar)]
    [TestCase(MultiLineInputWithSqlcmdStatementsWithoutCommentedOutSetVar)]
    public async Task ModifyAsync_CorrectModification_Async(string input)
    {
        // Arrange
        IScriptModifier modifier = new RemoveSqlCmdStatementsModifier();
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
        await modifier.ModifyAsync(model);

        // Assert
        model.CurrentScript.Should().Be(MultiLineInputWithoutSqlcmdStatements);
    }
}