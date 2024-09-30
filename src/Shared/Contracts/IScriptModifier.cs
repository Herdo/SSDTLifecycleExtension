namespace SSDTLifecycleExtension.Shared.Contracts;

public interface IScriptModifier
{
    /// <summary>
    ///     Modifies the <see cref="ScriptModificationModel.CurrentScript" /> of the <paramref name="model" />.
    /// </summary>
    /// <param name="model">The <see cref="ScriptModificationModel" /> containing all relevant data..</param>
    /// <returns>An awaitable task.</returns>
    Task ModifyAsync(ScriptModificationModel model);
}