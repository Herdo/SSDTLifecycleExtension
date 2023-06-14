namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

public class RemoveSqlCmdStatementsModifier : StringSearchModifierBase,
    IScriptModifier
{
    private static Task RemoveSqlCmdStatementsInternal(ScriptModificationModel model)
    {
        model.CurrentScript = ForEachMatch(model.CurrentScript,
                                           "USE [$(DatabaseName)];",
                                           0,
                                           s => string.Empty);
        model.CurrentScript = ForEachMatch(model.CurrentScript,
                                           "IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'",
                                           0,
                                           s => string.Empty);
        model.CurrentScript = ForEachMatch(model.CurrentScript,
                                           @":setvar __IsSqlCmdEnabled ""True""",
                                           0,
                                           s => string.Empty);
        model.CurrentScript = ForEachMatch(model.CurrentScript,
                                           ":on error exit",
                                           0,
                                           s => string.Empty);
        model.CurrentScript = ForEachMatch(model.CurrentScript,
                                           ":setvar DatabaseName",
                                           0,
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