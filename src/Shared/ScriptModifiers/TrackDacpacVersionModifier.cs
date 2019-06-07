namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Text;
    using Contracts;
    using Models;

    internal class TrackDacpacVersionModifier : IScriptModifier
    {
        private const string _CREATE_AND_INSERT_SCRIPT_TEMPLATE =
@"
IF OBJECT_ID(N'[dbo].[__DacpacVersion]', N'U') IS NULL
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
GO";

        private const string _UPDATE_SCRIPT_TEMPLATE =
@"
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
GO";

        string IScriptModifier.Modify(string input,
                                      ConfigurationModel configuration,
                                      ScriptCreationVariables variables)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Prepare format string
            var majorVersion = variables.NewVersion.Major.ToString();
            var minorVersion = variables.NewVersion.Minor.ToString();
            var buildVersion = variables.NewVersion.Build == -1 ? "NULL" : variables.NewVersion.Build.ToString();
            var revisionVersion = variables.NewVersion.Revision == -1 ? "NULL" : variables.NewVersion.Revision.ToString();
            var createAndInsert = string.Format(_CREATE_AND_INSERT_SCRIPT_TEMPLATE,
                                                variables.SqlTargetName,
                                                majorVersion,
                                                minorVersion,
                                                buildVersion,
                                                revisionVersion);
            var update = string.Format(_UPDATE_SCRIPT_TEMPLATE,
                                       variables.SqlTargetName,
                                       majorVersion,
                                       minorVersion,
                                       buildVersion,
                                       revisionVersion);

            var sb = new StringBuilder();

            // Create table and insert
            sb.AppendLine(createAndInsert);
            sb.AppendLine();

            // Input
            sb.AppendLine(input);

            // Update existing entry
            sb.AppendLine(update);

            return sb.ToString();
        }
    }
}