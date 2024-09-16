namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers;

[TestFixture]
public class StringSearchModifierBaseTests
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

    private const string MultiLineInputWithFinalGoWithDifferentSchema =
        @"PRINT 'First statement';

GO
ALTER TABLE [config].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE [config].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'

GO";
    private const string MultiLineInputWithFinalGoWithoutOverlappingComments =
        @"PRINT 'First statement';

GO
ALTER TABLE /*[dbo].*/[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second statement'

GO
ALTER TABLE /*[dbo].*/[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'

GO";

    private const string MultiLineInputWithFinalGoWithDifferentSchemaAndPrints =
        @"PRINT 'First go';

GO
ALTER TABLE [config].[Author] ADD COLUMN Birthday DATE NULL;

GO
PRINT 'Second go'

GO
ALTER TABLE [config].[Author] DROP COLUMN Birthday;

GO
PRINT 'Update complete'

GO";

    private const string MultiLineInputWithoutFinalGo =
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

    [Test]
    public void SearchStatementRange_ArgumentException_Statement()
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string input = @"";
        const string statement = @"";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => s.SearchStatementRangeBase(input, statement, 0, 0));
    }

    [Test]
    public void SearchStatementRange_ArgumentOutOfRangeException_StartAfterIndex()
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string input = @"";
        const string statement = @"a";

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => s.SearchStatementRangeBase(input, statement, -1, 0));
    }

    [Test]
    public void SearchStatementRange_NoMatch()
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string input = @"";
        const string statement = @"a";

        // Act
        var (startIndex, endIndex) = s.SearchStatementRangeBase(input, statement, 0, 0);

        // Assert
        startIndex.Should().Be(-1);
        endIndex.Should().Be(-1);
    }

    [Test]
    [TestCase(0, 0, 0, 32)]
    [TestCase(0, 1, 0, 32)]
    [TestCase(16, 0, -1, -1)]
    [TestCase(16, 1, -1, -1)]
    public void SearchStatementRange_SingleStatementAtStart(int startAfterIndex,
                                                            byte numberOfLeadingStatementsToInclude,
                                                            int expectedStartIndex,
                                                            int expectedEndIndex)
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"'First statement'";

        // Act
        var (startIndex, endIndex) = s.SearchStatementRangeBase(MultiLineInputWithFinalGo, statement, startAfterIndex, numberOfLeadingStatementsToInclude);

        // Assert
        startIndex.Should().Be(expectedStartIndex);
        endIndex.Should().Be(expectedEndIndex);
    }

    [Test]
    [TestCase(0, 0, 129, 185)]
    [TestCase(0, 1, 97, 185)]
    [TestCase(0, 2, 32, 185)]
    [TestCase(32, 0, 129, 185)]
    [TestCase(32, 1, 97, 185)]
    [TestCase(32, 2, 32, 185)]
    [TestCase(162, 0, -1, -1)]
    [TestCase(162, 1, -1, -1)]
    [TestCase(162, 2, -1, -1)]
    public void SearchStatementRange_SingleStatementInMiddle(int startAfterIndex,
                                                             byte numberOfLeadingStatementsToInclude,
                                                             int expectedStartIndex,
                                                             int expectedEndIndex)
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"DROP COLUMN";

        // Act
        var (startIndex, endIndex) = s.SearchStatementRangeBase(MultiLineInputWithFinalGo, statement, startAfterIndex, numberOfLeadingStatementsToInclude);

        // Assert
        startIndex.Should().Be(expectedStartIndex);
        endIndex.Should().Be(expectedEndIndex);
    }

    [Test]
    [TestCase(0, 0, 185, 214)]
    [TestCase(0, 1, 129, 214)]
    [TestCase(0, 2, 97, 214)]
    [TestCase(32, 0, 185, 214)]
    [TestCase(32, 1, 129, 214)]
    [TestCase(32, 2, 97, 214)]
    [TestCase(200, 0, -1, -1)]
    [TestCase(200, 1, -1, -1)]
    [TestCase(200, 2, -1, -1)]
    public void SearchStatementRange_SingleStatementAtEnd_WithFinalGo(int startAfterIndex,
                                                                      byte numberOfLeadingStatementsToInclude,
                                                                      int expectedStartIndex,
                                                                      int expectedEndIndex)
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"'Update complete'";

        // Act
        var (startIndex, endIndex) = s.SearchStatementRangeBase(MultiLineInputWithFinalGo, statement, startAfterIndex, numberOfLeadingStatementsToInclude);

        // Assert
        startIndex.Should().Be(expectedStartIndex);
        endIndex.Should().Be(expectedEndIndex);
    }

    [Test]
    [TestCase(0, 0, 185, 210)]
    [TestCase(0, 1, 129, 210)]
    [TestCase(0, 2, 97, 210)]
    [TestCase(32, 0, 185, 210)]
    [TestCase(32, 1, 129, 210)]
    [TestCase(32, 2, 97, 210)]
    [TestCase(200, 0, -1, -1)]
    [TestCase(200, 1, -1, -1)]
    [TestCase(200, 2, -1, -1)]
    public void SearchStatementRange_SingleStatementAtEnd_WithoutFinalGo(int startAfterIndex,
                                                                         byte numberOfLeadingStatementsToInclude,
                                                                         int expectedStartIndex,
                                                                         int expectedEndIndex)
    {
        // Arrange
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"'Update complete'";

        // Act
        var (startIndex, endIndex) = s.SearchStatementRangeBase(MultiLineInputWithoutFinalGo, statement, startAfterIndex, numberOfLeadingStatementsToInclude);

        // Assert
        startIndex.Should().Be(expectedStartIndex);
        endIndex.Should().Be(expectedEndIndex);
    }

    [Test]
    public void ForEachMatch_ArgumentException_Statement()
    {
        // Arrange
        var modifier = new Func<string, string>(range => range);
        var s = new StringSearchModifierBaseTestImplementation();
        const string input = @"";
        const string statement = @"";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => s.ForEachMatchBase(input, statement, 0, modifier));
    }

    [Test]
    public void ForEachMatch_InvalidOperationException_ModifiedIsNull()
    {
        // Arrange
        var modifier = new Func<string, string>(range => null);
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"[dbo].";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => s.ForEachMatchBase(MultiLineInputWithFinalGo, statement, 0, modifier));
    }

    [Test]
    public void ForEachMatch_NoMatch()
    {
        // Arrange
        var modifierCalled = false;
        var modifier = new Func<string, string>(range =>
        {
            modifierCalled = true;
            return range;
        });
        var s = new StringSearchModifierBaseTestImplementation();
        const string input = @"foobar";
        const string statement = @"test";

        // Act
        var modified = s.ForEachMatchBase(input, statement, 0, modifier);

        // Assert
        modifierCalled.Should().BeFalse();
        modified.Should().Be("foobar");
    }

    [Test]
    public void ForEachMatch_ReplaceMultipleMatches()
    {
        // Arrange
        var modifierCalled = false;
        var modifier = new Func<string, string>(range =>
        {
            modifierCalled = true;
            return range.Replace("dbo", "config");
        });
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"[dbo].";

        // Act
        var modified = s.ForEachMatchBase(MultiLineInputWithFinalGo, statement, 0, modifier);

        // Assert
        modifierCalled.Should().BeTrue();
        modified.Should().Be(MultiLineInputWithFinalGoWithDifferentSchema);
    }

    [Test]
    public void ForEachMatch_ReplaceMultipleMatches_PreventOverlapping()
    {
        // Arrange
        var modifierCalled = false;
        var modifier = new Func<string, string>(range =>
        {
            modifierCalled = true;
            return range.Replace("[dbo].", "/*[dbo].*/");
        });
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"[dbo].";

        // Act
        var modified = s.ForEachMatchBase(MultiLineInputWithFinalGo, statement, 5, modifier);

        // Assert
        modifierCalled.Should().BeTrue();
        modified.Should().Be(MultiLineInputWithFinalGoWithoutOverlappingComments);
    }

    [Test]
    public void ForEachMatch_ReplaceMultipleMatchesIncludeLeadingStatement()
    {
        // Arrange
        var modifierCalled = false;
        var modifier = new Func<string, string>(range =>
        {
            modifierCalled = true;
            return range.Replace("dbo", "config").Replace("statement", "go");
        });
        var s = new StringSearchModifierBaseTestImplementation();
        const string statement = @"[dbo].";

        // Act
        var modified = s.ForEachMatchBase(MultiLineInputWithFinalGo, statement, 1, modifier);

        // Assert
        modifierCalled.Should().BeTrue();
        modified.Should().Be(MultiLineInputWithFinalGoWithDifferentSchemaAndPrints);
    }

    private class StringSearchModifierBaseTestImplementation : StringSearchModifierBase
    {
        internal (int StartIndex, int EndIndex) SearchStatementRangeBase(string input,
                                                                         string statement,
                                                                         int startAfterIndex,
                                                                         byte numberOfLeadingStatementsToInclude) =>
            SearchStatementRange(input, statement, startAfterIndex, numberOfLeadingStatementsToInclude);

        internal string ForEachMatchBase(string input,
                                         string statement,
                                         byte numberOfLeadingStatementsToInclude,
                                         Func<string, string> modifier) =>
            ForEachMatch(input, statement, numberOfLeadingStatementsToInclude, modifier);
    }
}