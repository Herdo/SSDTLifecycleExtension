namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Threading.Tasks;
    using Contracts;
    using Models;

    public class RemoveSqlCmdStatementsModifier : StringSearchModifierBase,
                                                  IScriptModifier
    {
        private static Task RemoveSqlCmdStatementsInternal(ScriptModificationModel model)
        {
            model.CurrentScript = ForEachMatch(model.CurrentScript,
                                               "USE [$(DatabaseName)];",
                                               4,
                                               s => string.Empty);
            return Task.CompletedTask;
        }

        Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            return RemoveSqlCmdStatementsInternal(model);
        }
    }
}