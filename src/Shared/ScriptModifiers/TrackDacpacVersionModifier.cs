namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

internal class TrackDacpacVersionModifier : StringSearchModifierBase, IScriptModifier
{
    private const string CreateAndInsertScriptTemplate =
        @"IF OBJECT_ID(N'[dbo].[__DacpacVersion]', N'U') IS NULL
BEGIN
	PRINT 'Creating table dbo.__DacpacVersion'
	CREATE TABLE [dbo].[__DacpacVersion]
	(
		DacpacVersionID	INT				NOT NULL	IDENTITY(1, 1),
		DacpacName		NVARCHAR(512)	NOT NULL,
		Major			INT				NOT NULL,
		Minor			INT				NOT NULL,
		Build			INT				NULL,
		Revision		INT				NULL,
		DeploymentStart	DATETIME2		NOT NULL,
		DeploymentEnd	DATETIME2		NULL,
		CONSTRAINT PK_DacpacVersion_DacpacVersionID PRIMARY KEY (DacpacVersionID)
	);
END

GO
PRINT 'Tracking version number for current deployment'

GO
INSERT INTO [dbo].[__DacpacVersion]
		   (DacpacName, Major, Minor, Build, Revision, DeploymentStart)
	VALUES
		(
			N'{0}',
			{1},
			{2},
			NULLIF({3}, -1),
			NULLIF({4}, -1),
			SYSDATETIME()
		);

GO
";

    private const string UpdateScriptTemplate =
        @"
GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'{0}'
   AND [dv].[Major] = {1}
   AND [dv].[Minor] = {2}
   AND ISNULL([dv].[Build], -1) = {3}
   AND ISNULL([dv].[Revision], -1) = {4};

GO
";

    private static (string CreateAndInsertStatement, string UpdateStatement) GetFinalStatementsFromTemplate(string input,
        SqlProject project)
    {
        var newVersion = project.ProjectProperties.DacVersion!;
        var majorVersion = newVersion.Major.ToString();
        var minorVersion = newVersion.Minor.ToString();
        var buildVersion = newVersion.Build.ToString();
        var revisionVersion = newVersion.Revision.ToString();
        var createAndInsertStatement = string.Format(CreateAndInsertScriptTemplate,
                                                     project.ProjectProperties.SqlTargetName,
                                                     majorVersion,
                                                     minorVersion,
                                                     buildVersion,
                                                     revisionVersion);
        var updateStatement = string.Format(UpdateScriptTemplate,
                                            project.ProjectProperties.SqlTargetName,
                                            majorVersion,
                                            minorVersion,
                                            buildVersion,
                                            revisionVersion);
        ChangeUpdateStatementDependingOnInputEnd(ref updateStatement, input);
        return (createAndInsertStatement, updateStatement);
    }

    private static void ChangeUpdateStatementDependingOnInputEnd(ref string update,
                                                                 string input)
    {
        if (input.EndsWith("GO"))
            update = Environment.NewLine + update.Substring(6);
        else if (input.EndsWith($"GO{Environment.NewLine}"))
            update = update.Substring(6);
        else if (!input.EndsWith(Environment.NewLine))
            update = Environment.NewLine + update;
    }

    Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
    {
        Guard.IsNotNull(model.Project.ProjectProperties.DacVersion);

        if (model.Project.ProjectProperties.SqlTargetName == null)
            throw new ArgumentException(
                $"{nameof(ScriptModificationModel.Project)}.{nameof(SqlProject.ProjectProperties)}.{nameof(SqlProjectProperties.SqlTargetName)} must be set.",
                nameof(model));
        if (model.Project.ProjectProperties.DacVersion == null)
            throw new ArgumentException(
                $"{nameof(ScriptModificationModel.Project)}.{nameof(SqlProject.ProjectProperties)}.{nameof(SqlProjectProperties.DacVersion)} must be set.",
                nameof(model));

        // Prepare format string
        var (createAndInsertStatement, updateStatement) = GetFinalStatementsFromTemplate(model.CurrentScript, model.Project);

        var createAndInsertStatementSet = false;
        var modifiedWithInsertAndCreateStatement = ForEachMatch(model.CurrentScript,
                                                                "USE [$(DatabaseName)];",
                                                                0,
                                                                s =>
                                                                {
                                                                    if (createAndInsertStatementSet)
                                                                        return s;
                                                                    createAndInsertStatementSet = true;
                                                                    return s + createAndInsertStatement;
                                                                });
        model.CurrentScript = modifiedWithInsertAndCreateStatement + updateStatement;

        return Task.CompletedTask;
    }
}