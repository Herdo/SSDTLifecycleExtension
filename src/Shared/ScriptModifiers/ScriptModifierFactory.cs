namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

public class ScriptModifierFactory(IDependencyResolver _dependencyResolver)
    : IScriptModifierFactory
{
    IScriptModifier IScriptModifierFactory.CreateScriptModifier(ScriptModifier scriptModifier)
    {
        return scriptModifier switch
        {
            ScriptModifier.AddCustomHeader => _dependencyResolver.Get<AddCustomHeaderModifier>(),
            ScriptModifier.AddCustomFooter => _dependencyResolver.Get<AddCustomFooterModifier>(),
            ScriptModifier.TrackDacpacVersion => _dependencyResolver.Get<TrackDacpacVersionModifier>(),
            ScriptModifier.CommentOutUnnamedDefaultConstraintDrops => _dependencyResolver.Get<CommentOutUnnamedDefaultConstraintDropsModifier>(),
            ScriptModifier.ReplaceUnnamedDefaultConstraintDrops => _dependencyResolver.Get<ReplaceUnnamedDefaultConstraintDropsModifier>(),
            ScriptModifier.RemoveSqlCmdStatements => _dependencyResolver.Get<RemoveSqlCmdStatementsModifier>(),
            _ => throw new ArgumentOutOfRangeException(nameof(scriptModifier), scriptModifier, null),
        };
    }
}