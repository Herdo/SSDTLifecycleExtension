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
        [NotNull] private readonly IDependencyResolver _dependencyResolver;

        public ScriptModifierFactory([NotNull] IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        }

        IScriptModifier IScriptModifierFactory.CreateScriptModifier(ScriptModifier scriptModifier)
        {
            switch (scriptModifier)
            {
                case ScriptModifier.AddCustomHeader:
                    return _dependencyResolver.Get<AddCustomHeaderModifier>();
                case ScriptModifier.AddCustomFooter:
                    return _dependencyResolver.Get<AddCustomFooterModifier>();
                case ScriptModifier.TrackDacpacVersion:
                    return _dependencyResolver.Get<TrackDacpacVersionModifier>();
                case ScriptModifier.CommentOutUnnamedDefaultConstraintDrops:
                    return _dependencyResolver.Get<CommentOutUnnamedDefaultConstraintDropsModifier>();
                case ScriptModifier.ReplaceUnnamedDefaultConstraintDrops:
                    return _dependencyResolver.Get<ReplaceUnnamedDefaultConstraintDropsModifier>();
                case ScriptModifier.RemoveSqlCmdStatements:
                    return _dependencyResolver.Get<RemoveSqlCmdStatementsModifier>();
                default:
                    throw new ArgumentOutOfRangeException(nameof(scriptModifier), scriptModifier, null);
            }
        }
    }
}