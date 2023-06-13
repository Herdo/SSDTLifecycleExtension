namespace SSDTLifecycleExtension.Shared.Contracts;

[DebuggerDisplay("{" + nameof(DisplayName) + ",nq}")]
public sealed class DefaultConstraint : IEquatable<DefaultConstraint>
{
    [NotNull] public string TableSchema { get; }

    [NotNull] public string TableName { get; }

    [NotNull] public string ColumnName { get; }

    [CanBeNull] public string ConstraintName { get; }

    [NotNull] public string DisplayName => $"[{TableSchema}].[{TableName}].[{ColumnName}].[{ConstraintName ?? "<UNNAMED>"}]";

    public DefaultConstraint([NotNull] string tableSchema,
                             [NotNull] string tableName,
                             [NotNull] string columnName,
                             [CanBeNull] string constraintName)
    {
        TableSchema = tableSchema ?? throw new ArgumentNullException(nameof(tableSchema));
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
        ConstraintName = constraintName;
    }

    public bool Equals(DefaultConstraint other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return string.Equals(TableSchema, other.TableSchema)
            && string.Equals(TableName, other.TableName)
            && string.Equals(ColumnName, other.ColumnName)
            && string.Equals(ConstraintName, other.ConstraintName);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((DefaultConstraint) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = TableSchema.GetHashCode();
            hashCode = (hashCode * 397) ^ TableName.GetHashCode();
            hashCode = (hashCode * 397) ^ ColumnName.GetHashCode();
            hashCode = (hashCode * 397) ^ (ConstraintName != null ? ConstraintName.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool operator ==(DefaultConstraint left,
                                   DefaultConstraint right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DefaultConstraint left,
                                   DefaultConstraint right)
    {
        return !Equals(left, right);
    }
}