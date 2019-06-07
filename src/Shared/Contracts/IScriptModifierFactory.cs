namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using ScriptModifiers;

    public interface IScriptModifierFactory
    {
        /// <summary>
        /// Creates the desired <paramref name="scriptModifier"/>.
        /// </summary>
        /// <param name="scriptModifier">The modifier to create.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="scriptModifier"/> equals <see cref="ScriptModifier.Undefined"/>.</exception>
        /// <returns>The created modifier instance.</returns>
        IScriptModifier CreateScriptModifier(ScriptModifier scriptModifier);
    }
}