namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.Models;

    public interface IScriptModifier
    {
        /// <summary>
        /// Modifies the <see cref="ScriptModificationModel.CurrentScript"/> of the <paramref name="model"/>.
        /// </summary>
        /// <param name="model">The <see cref="ScriptModificationModel"/> containing all relevant data..</param>
        /// <exception cref="ArgumentNullException"><paramref name="model"/> is <b>null</b>.</exception>
        /// <returns>An awaitable task.</returns>
        Task ModifyAsync([NotNull] ScriptModificationModel model);
    }
}