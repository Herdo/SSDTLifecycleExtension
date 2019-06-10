namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;

    public abstract class StringSearchModifierBase
    {
        protected (int StartIndex, int EndIndex) SearchStatementRange(string input, string statement)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            var statementIndex = input.IndexOf(statement, StringComparison.Ordinal);
            if (statementIndex < 0)
                return (-1, -1); // No match for the statement

            // Search for the previous GO statement, then get the index of the next line
            var startIndex = input.LastIndexOf("GO\r\n", statementIndex, StringComparison.Ordinal);
            if (startIndex == -1)
                startIndex = input.LastIndexOf("GO", statementIndex, StringComparison.Ordinal);
            startIndex = input.LastIndexOf("GO\r\n", startIndex, StringComparison.Ordinal);
            if (startIndex >= 0)
                startIndex += 4;
            else if (startIndex == -1)
            {
                startIndex = input.LastIndexOf("GO", startIndex, StringComparison.Ordinal);
                if (startIndex >= 0)
                    startIndex += 2;
            }

            // Search for the next GO statement, then get the index of the next line
            var endIndex = input.IndexOf("GO\r\n", statementIndex, StringComparison.Ordinal);
            if (endIndex == -1)
                endIndex = input.IndexOf("GO", statementIndex, StringComparison.Ordinal);

            return (startIndex, endIndex);
        }
    }
}