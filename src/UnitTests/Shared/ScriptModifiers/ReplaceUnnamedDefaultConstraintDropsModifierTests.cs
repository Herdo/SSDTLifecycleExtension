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

        private const string MultipleDropDefaultConstraintStatementsReplaced =
@"-- PRINT 'Dropping unnamed DEFAULT constraint on dbo.Author ...';

-- GO
-- ALTER TABLE [dbo].[Author] DROP CONSTRAINT ;

-- GO
-- PRINT 'Dropping unnamed DEFAULT constraint on dbo.Book ...'

-- GO
-- ALTER TABLE [dbo].[Book] DROP CONSTRAINT ;

-- GO
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
        public async Task ModifyAsync_CorrectReplacement_Async()
        {
            // Arrange
            var daMock = new Mock<IDacAccess>();
            var loggerMock = new Mock<ILogger>();
            // TODO: daMock.Setup(m => m.)
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
            var paths = new PathCollection("a", "b", "c", "d", null, null);
            IScriptModifier modifier = new ReplaceUnnamedDefaultConstraintDropsModifier(daMock.Object, loggerMock.Object);

            // Act
            var result = await modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, config, paths);

            // Assert
            Assert.AreEqual(MultipleDropDefaultConstraintStatementsReplaced, result);
        }
    }
}