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
                                      ConfigurationModel configuration)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var (startIndex, endIndex) = SearchStatementRange(input, "DROP CONSTRAINT ;");
            if (startIndex == -1 || endIndex == -1)
                return input;

            // TODO
            return input;
        }
    }
}