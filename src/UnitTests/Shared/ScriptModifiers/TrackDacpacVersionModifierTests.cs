using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

    [TestFixture]
    public class TrackDacpacVersionModifierTests
    {
        private const string MultiLineInputWithFinalGo =
@"PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'

GO";

        private const string MultiLineInputWithFinalGoAndNewLine =
            @"PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'

GO
";

        private const string MultiLineInputWithoutFinalGo =
            @"PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'";

        private const string MultiLineInputWithoutFinalGoButWithNewLine =
            @"PRINT 'First statement';

GO
ALTER TABLE [dbo].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [dbo].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'
";

        private const string FinalMultilineStatementFullVersion =
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
			N'Database.TestProject',
			500,
			30,
			NULLIF(44, -1),
			NULLIF(80, -1),
			SYSDATETIME()
		);

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

GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'Database.TestProject'
   AND [dv].[Major] = 500
   AND [dv].[Minor] = 30
   AND ISNULL([dv].[Build], -1) = 44
   AND ISNULL([dv].[Revision], -1) = 80;

GO
";

        private const string FinalMultilineStatementMajorMinorBuildVersion =
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
			N'Database.TestProject',
			500,
			30,
			NULLIF(44, -1),
			NULLIF(-1, -1),
			SYSDATETIME()
		);

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

GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'Database.TestProject'
   AND [dv].[Major] = 500
   AND [dv].[Minor] = 30
   AND ISNULL([dv].[Build], -1) = 44
   AND ISNULL([dv].[Revision], -1) = -1;

GO
";

        private const string FinalMultilineStatementMajorMinorVersion =
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
			N'Database.TestProject',
			500,
			30,
			NULLIF(-1, -1),
			NULLIF(-1, -1),
			SYSDATETIME()
		);

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

GO
PRINT 'Tracking deployment execution time for current deployment'

GO
UPDATE [dv]
   SET [dv].[DeploymentEnd] = SYSDATETIME()
  FROM [dbo].[__DacpacVersion] AS [dv]
 WHERE [dv].[DacpacName] = N'Database.TestProject'
   AND [dv].[Major] = 500
   AND [dv].[Minor] = 30
   AND ISNULL([dv].[Build], -1) = -1
   AND ISNULL([dv].[Revision], -1) = -1;

GO
";

        [Test]
        public void Modify_ArgumentNullException_Input()
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.Modify(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Project()
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.Modify(MultiLineInputWithFinalGo, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Configuration()
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.Modify(MultiLineInputWithFinalGo, project, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Paths()
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");
            var configuration = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.Modify(MultiLineInputWithFinalGo, project, configuration, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentException_ProjectPropertiesSqlTargetNameNull()
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("", "", "", "", "", "");

            // Act
            var e = Assert.Throws<ArgumentException>(() => modifier.Modify(MultiLineInputWithFinalGo, project, configuration, paths));

            // Assert
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains(nameof(SqlProjectProperties.SqlTargetName)));
        }

        [Test]
        public void Modify_ArgumentException_ProjectPropertiesDacVersionNull()
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");
            project.ProjectProperties.SqlTargetName = "Database.TestProject";
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("", "", "", "", "", "");

            // Act
            var e = Assert.Throws<ArgumentException>(() => modifier.Modify(MultiLineInputWithFinalGo, project, configuration, paths));

            // Assert
            Assert.IsNotNull(e);
            Assert.IsTrue(e.Message.Contains(nameof(SqlProjectProperties.DacVersion)));
        }

        [Test]
        [TestCase(MultiLineInputWithFinalGo, TestName = nameof(MultiLineInputWithFinalGo))]
        [TestCase(MultiLineInputWithFinalGoAndNewLine, TestName = nameof(MultiLineInputWithFinalGoAndNewLine))]
        [TestCase(MultiLineInputWithoutFinalGo, TestName = nameof(MultiLineInputWithoutFinalGo))]
        [TestCase(MultiLineInputWithoutFinalGoButWithNewLine, TestName = nameof(MultiLineInputWithoutFinalGoButWithNewLine))]
        public void Modify_CorrectModification_FullVersion(string input)
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");
            project.ProjectProperties.SqlTargetName = "Database.TestProject";
            project.ProjectProperties.DacVersion = new Version(500, 30, 44, 80);
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("", "", "", "", "", "");

            // Act
            var modified = modifier.Modify(input, project, configuration, paths);

            // Assert
            Assert.AreEqual(FinalMultilineStatementFullVersion, modified);
        }

        [Test]
        [TestCase(MultiLineInputWithFinalGo, TestName = nameof(MultiLineInputWithFinalGo))]
        [TestCase(MultiLineInputWithFinalGoAndNewLine, TestName = nameof(MultiLineInputWithFinalGoAndNewLine))]
        [TestCase(MultiLineInputWithoutFinalGo, TestName = nameof(MultiLineInputWithoutFinalGo))]
        [TestCase(MultiLineInputWithoutFinalGoButWithNewLine, TestName = nameof(MultiLineInputWithoutFinalGoButWithNewLine))]
        public void Modify_CorrectModification_MajorMinorBuildVersion(string input)
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");
            project.ProjectProperties.SqlTargetName = "Database.TestProject";
            project.ProjectProperties.DacVersion = new Version(500, 30, 44);
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("", "", "", "", "", "");

            // Act
            var modified = modifier.Modify(input, project, configuration, paths);

            // Assert
            Assert.AreEqual(FinalMultilineStatementMajorMinorBuildVersion, modified);
        }

        [Test]
        [TestCase(MultiLineInputWithFinalGo, TestName = nameof(MultiLineInputWithFinalGo))]
        [TestCase(MultiLineInputWithFinalGoAndNewLine, TestName = nameof(MultiLineInputWithFinalGoAndNewLine))]
        [TestCase(MultiLineInputWithoutFinalGo, TestName = nameof(MultiLineInputWithoutFinalGo))]
        [TestCase(MultiLineInputWithoutFinalGoButWithNewLine, TestName = nameof(MultiLineInputWithoutFinalGoButWithNewLine))]
        public void Modify_CorrectModification_MajorMinorVersion(string input)
        {
            // Arrange
            IScriptModifier modifier = new TrackDacpacVersionModifier();
            var project = new SqlProject("", "", "");
            project.ProjectProperties.SqlTargetName = "Database.TestProject";
            project.ProjectProperties.DacVersion = new Version(500, 30);
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("", "", "", "", "", "");

            // Act
            var modified = modifier.Modify(input, project, configuration, paths);

            // Assert
            Assert.AreEqual(FinalMultilineStatementMajorMinorVersion, modified);
        }
    }
}