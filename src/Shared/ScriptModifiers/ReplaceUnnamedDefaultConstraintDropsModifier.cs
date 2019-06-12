namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using Contracts;
    using Models;

    public class ReplaceUnnamedDefaultConstraintDropsModifier : StringSearchModifierBase,
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

            var (startIndex, endIndex) = SearchStatementRange(input, "DROP CONSTRAINT ;", 0, 1);
            if (startIndex == -1 || endIndex == -1)
                return input;

            var pre = input.Substring(0, startIndex);
            var post = input.Substring(endIndex);

            // Unpack base version ZIP and scan the model.xml
            // TODO

            return pre + "" + post;
        }
    }
}