namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using Contracts;
    using Contracts.DataAccess;
    using Contracts.Enums;
    using Contracts.Factories;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class ScriptModifierFactory : IScriptModifierFactory
    {
        private readonly IDacAccess _dacAccess;
        private readonly ILogger _logger;

        public ScriptModifierFactory([NotNull] IDacAccess dacAccess,
                                     [NotNull] ILogger logger)
        {
            _dacAccess = dacAccess ?? throw new ArgumentNullException(nameof(dacAccess));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
                    return new ReplaceUnnamedDefaultConstraintDropsModifier(_dacAccess, _logger);
                default:
                    throw new ArgumentOutOfRangeException(nameof(scriptModifier), scriptModifier, null);
            }
        }
    }
}