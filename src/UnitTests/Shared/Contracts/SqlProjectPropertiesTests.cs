using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class SqlProjectPropertiesTests
    {
        [Test]
        public void Default_AllPropertiesNull()
        {
            // Act
            var p = new SqlProjectProperties();

            // Assert
            Assert.IsNull(p.SqlTargetName);
            Assert.IsNull(p.BinaryDirectory);
            Assert.IsNull(p.DacVersion);
        }

        [Test]
        public void SqlTargetName_SetGet()
        {
            // Arrange
            var p = new SqlProjectProperties();
            const string sqlTargetName = "sqlTarget";

            // Act
            p.SqlTargetName = sqlTargetName;

            // Assert
            Assert.AreEqual(sqlTargetName, p.SqlTargetName);
        }

        [Test]
        public void BinaryDirectory_SetGet()
        {
            // Arrange
            var p = new SqlProjectProperties();
            const string binaryDirectoryName = "binaryDirectory";

            // Act
            p.BinaryDirectory = binaryDirectoryName;

            // Assert
            Assert.AreEqual(binaryDirectoryName, p.BinaryDirectory);
        }

        [Test]
        public void DacVersion_SetGet()
        {
            // Arrange
            var p = new SqlProjectProperties();
            var v = new Version(2, 3, 0, 1);

            // Act
            p.DacVersion = v;

            // Assert
            Assert.AreSame(v, p.DacVersion);
        }
    }
}