namespace SSDTLifecycleExtension.UnitTests.Shared.ScriptModifiers
{
    using System;
    using NUnit.Framework;
    using SSDTLifecycleExtension.Shared.ScriptModifiers;

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
        public void SearchStatementRange_ArgumentNullException_Input()
        {
            // Arrange
            var s = new StringSearchModifierBaseTestImplementation();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.SearchStatementRangeBase(null, null, 0, 0));
        }

        [Test]
        public void SearchStatementRange_ArgumentNullException_Statement()
        {
            // Arrange
            var s = new StringSearchModifierBaseTestImplementation();
            const string input = @"";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => s.SearchStatementRangeBase(input, null, 0, 0));
        }

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
            Assert.AreEqual(-1, startIndex, "Wrong start index");
            Assert.AreEqual(-1, endIndex, "Wrong end index");
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
            Assert.AreEqual(expectedStartIndex, startIndex, "Wrong start index");
            Assert.AreEqual(expectedEndIndex, endIndex, "Wrong end index");
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
            Assert.AreEqual(expectedStartIndex, startIndex, "Wrong start index");
            Assert.AreEqual(expectedEndIndex, endIndex, "Wrong end index");
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
            Assert.AreEqual(expectedStartIndex, startIndex, "Wrong start index");
            Assert.AreEqual(expectedEndIndex, endIndex, "Wrong end index");
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
            Assert.AreEqual(expectedStartIndex, startIndex, "Wrong start index");
            Assert.AreEqual(expectedEndIndex, endIndex, "Wrong end index");
        }

        private class StringSearchModifierBaseTestImplementation : StringSearchModifierBase
        {
            internal (int StartIndex, int EndIndex) SearchStatementRangeBase(string input,
                                                                             string statement,
                                                                             int startAfterIndex,
                                                                             byte numberOfLeadingStatementsToInclude) =>
                SearchStatementRange(input, statement, startAfterIndex, numberOfLeadingStatementsToInclude);
        }
    }
}