using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using System.Threading.Tasks;
    using SSDTLifecycleExtension.Shared.Contracts;
    using SSDTLifecycleExtension.Shared.Models;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

    [TestFixture]
    public class CommentOutUnnamedDefaultConstraintDropsModifierTests
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

        private const string MultipleDropDefaultConstraintStatementsCommented =
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
        public void Modify_ArgumentNullException_Input()
        {
            // Arrange
            IScriptModifier modifier = new CommentOutUnnamedDefaultConstraintDropsModifier();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Project()
        {
            // Arrange
            IScriptModifier modifier = new CommentOutUnnamedDefaultConstraintDropsModifier();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Configuration()
        {
            // Arrange
            IScriptModifier modifier = new CommentOutUnnamedDefaultConstraintDropsModifier();
            var project = new SqlProject("", "", "");

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Modify_ArgumentNullException_Paths()
        {
            // Arrange
            IScriptModifier modifier = new CommentOutUnnamedDefaultConstraintDropsModifier();
            var project = new SqlProject("", "", "");
            var configuration = new ConfigurationModel();

            // Act & Assert
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, configuration, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public async Task Modify_CorrectModification_Async()
        {
            // Arrange
            IScriptModifier modifier = new CommentOutUnnamedDefaultConstraintDropsModifier();
            var project = new SqlProject("", "", "");
            var configuration = new ConfigurationModel();
            var paths = new PathCollection("", "", "", "", "", "");

            // Act
            var modified = await modifier.ModifyAsync(MultipleDropDefaultConstraintStatements, project, configuration, paths);

            // Assert
            Assert.AreEqual(MultipleDropDefaultConstraintStatementsCommented, modified);
        }
    }
}