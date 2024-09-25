namespace SSDTLifecycleExtension.UnitTests.Shared.Contracts;

[TestFixture]
public class DefaultConstraintTests
{
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
        dc.TableSchema.Should().Be(tableSchema);
        dc.TableName.Should().Be(tableName);
        dc.ColumnName.Should().Be(columnName);
        dc.ConstraintName.Should().Be(constraintName);
        dc.DisplayName.Should().Be($"[dbo].[Author].[Name].[{(constraintName ?? "<UNNAMED>")}]");
    }

    [Test]
    public void Equals_Object_Null_False()
    {
        // Arrange
        var instance = new DefaultConstraint("a", "b", "c", "d");

        // Act
        var equals = instance.Equals(null as object);

        // Assert
        equals.Should().BeFalse();
    }

    [Test]
    public void Equals_Object_SameInstance_True()
    {
        // Arrange
        var instance = new DefaultConstraint("a", "b", "c", "d");

        // Act
        var equals = instance.Equals((object)instance);

        // Assert
        equals.Should().BeTrue();
    }

    [Test]
    public void Equals_Object_DifferentType_False()
    {
        // Arrange
        var instance = new DefaultConstraint("a", "b", "c", "d");

        // Act
        var equals = instance.Equals(new Version(1, 0));

        // Assert
        equals.Should().BeFalse();
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
        equals.Should().Be(expectedEquals);
    }

    [Test]
    public void Equals_Typed_Null_False()
    {
        // Arrange
        var instance = new DefaultConstraint("a", "b", "c", "d");

        // Act
        var equals = instance.Equals(null);

        // Assert
        equals.Should().BeFalse();
    }

    [Test]
    public void Equals_Typed_SameInstance_True()
    {
        // Arrange
        var instance = new DefaultConstraint("a", "b", "c", "d");

        // Act
        var equals = instance.Equals(instance);

        // Assert
        equals.Should().BeTrue();
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
        equals.Should().Be(expectedEquals);
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
        sameHashCode.Should().Be(expectedSameHashCode);
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
        equality.Should().Be(expectedEquality);
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
        inequality.Should().Be(expectedInequality);
    }
}
