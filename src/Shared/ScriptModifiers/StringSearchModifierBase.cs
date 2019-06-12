namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;

    public abstract class StringSearchModifierBase
    {
        /// <summary>
        /// Searches the <paramref name="input"/> for the <paramref name="statement"/>.
        /// </summary>
        /// <param name="input">The text to search in.</param>
        /// <param name="statement">The text to search for.</param>
        /// <param name="startAfterIndex">Start searching after this index.</param>
        /// <param name="numberOfLeadingStatementsToInclude">The number of leading statements to include in the range. See example</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/> or <paramref name="statement"/> are <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="statement"/> contains only white spaces.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="startAfterIndex"/> is negative.</exception>
        /// <returns>(-1, -1), if no match is found, otherwise the start index and end index of the range.</returns>
        /// <remarks>The <b>EndIndex</b> might equal <paramref name="input"/>.<see cref="string.Length"/>, when no final GO statement can be found.</remarks>
        /// <example>Given this SQL:
        /// 1:    PRINT 'Hello world'
        /// 2:    
        /// 3:    GO
        /// 4:    ALTER TABLE [dbo].[Author] ADD COLUMN Born DATE NOT NULL;
        /// 5:    
        /// 6:    GO
        /// 7:    PRINT 'Bye world'
        /// <paramref name="numberOfLeadingStatementsToInclude"/> = 0 would include the beginning of line 4 until the beginning of line 7.
        /// <paramref name="numberOfLeadingStatementsToInclude"/> = 1 or higher would include the beginning of line 1 until the beginning of line 7.</example>
        protected (int StartIndex, int EndIndex) SearchStatementRange(string input,
                                                                      string statement,
                                                                      int startAfterIndex,
                                                                      byte numberOfLeadingStatementsToInclude)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (statement == null)
                throw new ArgumentNullException(nameof(statement));
            if (string.IsNullOrWhiteSpace(statement))
                throw new ArgumentException("Parameter may not be only white spaces.", nameof(statement));
            if (startAfterIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startAfterIndex));

            var statementIndex = input.IndexOf(statement, startAfterIndex, StringComparison.Ordinal);
            if (statementIndex < 0)
                return (-1, -1); // No match for the statement

            // Search for GO statements
            var startIndex = -1;
            var searchBefore = statementIndex;
            for (var i = 0; i <= numberOfLeadingStatementsToInclude; i++)
            {
                var indexOfPreviousGo = input.LastIndexOf("GO", searchBefore, StringComparison.Ordinal);
                if (i == numberOfLeadingStatementsToInclude)
                {
                    // Finish search
                    if (indexOfPreviousGo == -1)
                    {
                        // When no GO statement is found, we start the the beginning of input
                        startIndex = 0;
                    }
                    else
                    {
                        // When a GO statement is found, determine if it has a trailing line break or not,
                        // and get the index of after the GO (with line break).
                        var goWithLineBreak = input.Substring(indexOfPreviousGo, 4) == "GO\r\n";
                        startIndex = indexOfPreviousGo + (goWithLineBreak ? 4 : 2);
                    }
                    break;
                }

                // Search before this one, if this was a match
                if (indexOfPreviousGo >= 0)
                    searchBefore = indexOfPreviousGo;
            }

            // Search for the next GO statement, then get the index of the next line
            var endIndex = input.IndexOf("GO\r\n", statementIndex, StringComparison.Ordinal);
            if (endIndex >= 0)
            {
                // End after the new line
                endIndex += 4;
            }
            else
            {
                endIndex = input.IndexOf("GO", statementIndex, StringComparison.Ordinal);
                if (endIndex >= 0)
                {
                    // End after the GO
                    endIndex += 2;
                }
                else
                {
                    // No trailing GO, end at the end of input
                    endIndex = input.Length;
                }
            }

            return (startIndex, endIndex);
        }
    }
}