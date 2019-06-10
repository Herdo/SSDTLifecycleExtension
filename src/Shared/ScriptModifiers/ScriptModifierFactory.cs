namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using Contracts;
    using Contracts.Enums;
    using Contracts.Factories;
    using JetBrains.Annotations;

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
                case ScriptModifier.CommentOutUnnamedDefaultConstraintDrops:
                    return new CommentOutUnnamedDefaultConstraintDropsModifier();
                case ScriptModifier.ReplaceUnnamedDefaultConstraintDrops:
                    return new ReplaceUnnamedDefaultConstraintDropsModifier();
                default:
                    throw new ArgumentOutOfRangeException(nameof(scriptModifier), scriptModifier, null);
            }
        }
    }
}