namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Contracts;
    using Models;

    public class CommentOutUnnamedDefaultConstraintDropsModifier : StringSearchModifierBase,
                                                                   IScriptModifier
    {
        Task<string> IScriptModifier.ModifyAsync(string input,
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

            return Task.FromResult(ForEachMatch(input,
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
                                                }));
        }
    }
}