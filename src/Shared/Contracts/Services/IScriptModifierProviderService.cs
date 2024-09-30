namespace SSDTLifecycleExtension.Shared.Contracts.Services;

public interface IScriptModifierProviderService
{
    /// <summary>
    ///     Provides the <see cref="IScriptModifier" />s for the given <paramref name="configuration" />.
    /// </summary>
    /// <param name="configuration">The <see cref="ConfigurationModel" /> to provide the script modifiers for.</param>
    /// <returns>A dictionary containing all configured modifiers.</returns>
    IReadOnlyDictionary<ScriptModifier, IScriptModifier> GetScriptModifiers(ConfigurationModel configuration);
}