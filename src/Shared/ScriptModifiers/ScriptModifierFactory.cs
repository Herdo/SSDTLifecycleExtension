namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;

    [UsedImplicitly]
    public class ScriptModifierFactory : IScriptModifierFactory
    {
        IScriptModifier IScriptModifierFactory.CreateScriptModifier(ScriptModifier scriptModifier)
        {
            switch (scriptModifier)
            {
                case ScriptModifier.AddCustomHeader:
                    return new AddCustomHeaderModifier();
                case ScriptModifier.AddCustomFooter:
                    return new AddCustomFooterModifier();
                case ScriptModifier.TrackDacpacVersion:
                    return new TrackDacpacVersionModifier();
                default:
                    throw new ArgumentOutOfRangeException(nameof(scriptModifier), scriptModifier, null);
            }
        }
    }
}