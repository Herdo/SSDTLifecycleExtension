using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts
{
    using System;
    using Microsoft.VisualStudio.Settings;
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

        [Test]
        public void Equals_Object_Null_False()
        {
            // Arrange
            var instance = new DefaultConstraint("a", "b", "c", "d");

            // Act
            var equals = instance.Equals(null as object);

            // Assert
            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_Object_SameInstance_True()
        {
            // Arrange
            var instance = new DefaultConstraint("a", "b", "c", "d");

            // Act
            var equals = instance.Equals((object) instance);

            // Assert
            Assert.IsTrue(equals);
        }

        [Test]
        public void Equals_Object_DifferentType_False()
        {
            // Arrange
            var instance = new DefaultConstraint("a", "b", "c", "d");

            // Act
            var equals = instance.Equals(new Version(1, 0));

            // Assert
            Assert.IsFalse(equals);
        }

        [Test]
        [TestCase("a", "b", "c", "d", true)]
        [TestCase("a", "b", "c", null, false)]
        [TestCase("a1", "b", "c", "d", false)]
        [TestCase("a", "b1", "c", "d", false)]
        [TestCase("a", "b", "c1", "d", false)]
        [TestCase("a", "b", "c", "d1", false)]
        public void Equals_Object_TwoInstances(string tableSchema, string tableName, string columnName, string constraintName, bool expectedEquals)
        {
            // Arrange
            var instance1 = new DefaultConstraint("a", "b", "c", "d");
            var instance2 = new DefaultConstraint(tableSchema, tableName, columnName, constraintName);

            // Act
            var equals = instance1.Equals((object)instance2);

            // Assert
            Assert.AreEqual(expectedEquals, equals);
        }

        [Test]
        public void Equals_Typed_Null_False()
        {
            // Arrange
            var instance = new DefaultConstraint("a", "b", "c", "d");

            // Act
            var equals = instance.Equals(null);

            // Assert
            Assert.IsFalse(equals);
        }

        [Test]
        public void Equals_Typed_SameInstance_True()
        {
            // Arrange
            var instance = new DefaultConstraint("a", "b", "c", "d");

            // Act
            var equals = instance.Equals(instance);

            // Assert
            Assert.IsTrue(equals);
        }

        [Test]
        [TestCase("a", "b", "c", "d", true)]
        [TestCase("a", "b", "c", null, false)]
        [TestCase("a1", "b", "c", "d", false)]
        [TestCase("a", "b1", "c", "d", false)]
        [TestCase("a", "b", "c1", "d", false)]
        [TestCase("a", "b", "c", "d1", false)]
        public void Equals_Typed_TwoInstances(string tableSchema, string tableName, string columnName, string constraintName, bool expectedEquals)
        {
            // Arrange
            var instance1 = new DefaultConstraint("a", "b", "c", "d");
            var instance2 = new DefaultConstraint(tableSchema, tableName, columnName, constraintName);

            // Act
            var equals = instance1.Equals(instance2);

            // Assert
            Assert.AreEqual(expectedEquals, equals);
        }

        [Test]
        [TestCase("a", "b", "c", "d", true)]
        [TestCase("a", "b", "c", null, false)]
        [TestCase("a1", "b", "c", "d", false)]
        [TestCase("a", "b1", "c", "d", false)]
        [TestCase("a", "b", "c1", "d", false)]
        [TestCase("a", "b", "c", "d1", false)]
        public void GetHashCode_HashCodeComparison(string tableSchema, string tableName, string columnName, string constraintName, bool expectedSameHashCode)
        {
            // Arrange
            var instance1 = new DefaultConstraint("a", "b", "c", "d");
            var instance2 = new DefaultConstraint(tableSchema, tableName, columnName, constraintName);

            // Act
            var i1HashCode = instance1.GetHashCode();
            var i2HashCode = instance2.GetHashCode();
            var sameHashCode = i1HashCode == i2HashCode;

            // Assert
            Assert.AreEqual(expectedSameHashCode, sameHashCode);
        }

        [Test]
        [TestCase("a", "b", "c", "d", true)]
        [TestCase("a", "b", "c", null, false)]
        [TestCase("a1", "b", "c", "d", false)]
        [TestCase("a", "b1", "c", "d", false)]
        [TestCase("a", "b", "c1", "d", false)]
        [TestCase("a", "b", "c", "d1", false)]
        public void EqualityOperator(string tableSchema, string tableName, string columnName, string constraintName, bool expectedEquality)
        {
            // Arrange
            var instance1 = new DefaultConstraint("a", "b", "c", "d");
            var instance2 = new DefaultConstraint(tableSchema, tableName, columnName, constraintName);

            // Act
            var equality = instance1 == instance2;

            // Assert
            Assert.AreEqual(expectedEquality, equality);
        }

        [Test]
        [TestCase("a", "b", "c", "d", false)]
        [TestCase("a", "b", "c", null, true)]
        [TestCase("a1", "b", "c", "d", true)]
        [TestCase("a", "b1", "c", "d", true)]
        [TestCase("a", "b", "c1", "d", true)]
        [TestCase("a", "b", "c", "d1", true)]
        public void InequalityOperator(string tableSchema, string tableName, string columnName, string constraintName, bool expectedInequality)
        {
            // Arrange
            var instance1 = new DefaultConstraint("a", "b", "c", "d");
            var instance2 = new DefaultConstraint(tableSchema, tableName, columnName, constraintName);

            // Act
            var inequality = instance1 != instance2;

            // Assert
            Assert.AreEqual(expectedInequality, inequality);
        }
    }
}