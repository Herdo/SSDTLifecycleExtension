namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

public class CommentOutUnnamedDefaultConstraintDropsModifier : StringSearchModifierBase,
    IScriptModifier
{
    Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        model.CurrentScript = ForEachMatch(model.CurrentScript,
                                           "DROP CONSTRAINT ;",
                                           1,
                                           range =>
                                           {
                                               var lines = range.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
                                               var newLines = lines.Select(m => string.IsNullOrWhiteSpace(m)
                                                                               ? m
                                                                               : $"-- {m}").ToArray();
                                               var replacement = string.Join(Environment.NewLine, newLines);
                                               return replacement;
                                           });
        return Task.CompletedTask;
    }
}