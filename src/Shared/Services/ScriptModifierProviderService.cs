namespace SSDTLifecycleExtension.Shared.Services;

public class ScriptModifierProviderService(IScriptModifierFactory _scriptModifierFactory)
    : IScriptModifierProviderService
{
    IReadOnlyDictionary<ScriptModifier, IScriptModifier> IScriptModifierProviderService.GetScriptModifiers(ConfigurationModel configuration)
    {
        var result = new Dictionary<ScriptModifier, IScriptModifier>();

        if (configuration.CommentOutUnnamedDefaultConstraintDrops)
            result[ScriptModifier.CommentOutUnnamedDefaultConstraintDrops] =
                _scriptModifierFactory.CreateScriptModifier(ScriptModifier.CommentOutUnnamedDefaultConstraintDrops);

        if (configuration.ReplaceUnnamedDefaultConstraintDrops)
            result[ScriptModifier.ReplaceUnnamedDefaultConstraintDrops] =
                _scriptModifierFactory.CreateScriptModifier(ScriptModifier.ReplaceUnnamedDefaultConstraintDrops);

        if (!string.IsNullOrWhiteSpace(configuration.CustomHeader))
            result[ScriptModifier.AddCustomHeader] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomHeader);

        if (!string.IsNullOrWhiteSpace(configuration.CustomFooter))
            result[ScriptModifier.AddCustomFooter] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.AddCustomFooter);

        if (configuration.TrackDacpacVersion)
            result[ScriptModifier.TrackDacpacVersion] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.TrackDacpacVersion);

        if (configuration.RemoveSqlCmdStatements)
            result[ScriptModifier.RemoveSqlCmdStatements] = _scriptModifierFactory.CreateScriptModifier(ScriptModifier.RemoveSqlCmdStatements);

        return result;
    }
}