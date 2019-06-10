namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Linq;
    using Contracts;
    using Models;

    public class CommentOutUnnamedDefaultConstraintDropsModifier : StringSearchModifierBase,
                                                                   IScriptModifier
    {
        string IScriptModifier.Modify(string input,
                                      SqlProject project,
                                      ConfigurationModel configuration,
                                      PathCollection paths)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            var (startIndex, endIndex) = SearchStatementRange(input, "DROP CONSTRAINT ;");
            if (startIndex == -1 || endIndex == -1)
                return input;

            var pre = input.Substring(0, startIndex);
            var post = input.Substring(endIndex);
            var range = input.Substring(startIndex, endIndex - startIndex);
            var lines = range.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            var newLines = lines.Select(m => $"-- {m}").ToArray();
            var replacement = string.Join(Environment.NewLine, newLines);
            return pre + replacement + post;
        }
    }
}