using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using SSDTLifecycleExtension.Shared.Contracts;

    [TestFixture]
    public class DefaultConstraintTests
    {
        [Test]
        public void Constructor_ArgumentNullException_TableSchema()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DefaultConstraint(null, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_TableName()
        {
            // Arrange
            const string tableSchema = "dbo";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DefaultConstraint(tableSchema, null, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        public void Constructor_ArgumentNullException_ColumnName()
        {
            // Arrange
            const string tableSchema = "dbo";
            const string tableName = "Author";

            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new DefaultConstraint(tableSchema, tableName, null, null));
            // ReSharper restore AssignNullToNotNullAttribute
        }

        [Test]
        [TestCase(null)]
        [TestCase("DF_Name")]
        public void Constructor_CorrectInitialization(string constraintName)
        {
            // Arrange
            const string tableSchema = "dbo";
            const string tableName = "Author";
            const string columnName = "Name";

            // Act
            var dc = new DefaultConstraint(tableSchema, tableName, columnName, constraintName);

            // Assert
            Assert.AreEqual(tableSchema, dc.TableSchema);
            Assert.AreEqual(tableName, dc.TableName);
            Assert.AreEqual(columnName, dc.ColumnName);
            Assert.AreEqual(constraintName, dc.ConstraintName);
        }
    }
}