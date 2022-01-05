using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class PublishProfileTests
    {
        [Test]
        public void CreateNewDatabase_Get_Set()
        {
            // Arrange
            var instance = new PublishProfile
            {
                CreateNewDatabase = false
            };

            // Act
            instance.CreateNewDatabase = true;

            // Assert
            Assert.IsTrue(instance.CreateNewDatabase);
        }

        [Test]
        public void BackupDatabaseBeforeChanges_Get_Set()
        {
            // Arrange
            var instance = new PublishProfile
            {
                BackupDatabaseBeforeChanges = false
            };

            // Act
            instance.BackupDatabaseBeforeChanges = true;

            // Assert
            Assert.IsTrue(instance.BackupDatabaseBeforeChanges);
        }

        [Test]
        public void ScriptDatabaseOptions_Get_Set()
        {
            // Arrange
            var instance = new PublishProfile
            {
                ScriptDatabaseOptions = false
            };

            // Act
            instance.ScriptDatabaseOptions = true;

            // Assert
            Assert.IsTrue(instance.ScriptDatabaseOptions);
        }

        [Test]
        public void ScriptDeployStateChecks_Get_Set()
        {
            // Arrange
            var instance = new PublishProfile
            {
                ScriptDeployStateChecks = false
            };

            // Act
            instance.ScriptDeployStateChecks = true;

            // Assert
            Assert.IsTrue(instance.ScriptDeployStateChecks);
        }
    }
}