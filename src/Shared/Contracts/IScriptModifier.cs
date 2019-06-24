namespace SSDTLifecycleExtension.Shared.Contracts
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.Models;

    public interface IScriptModifier
    {
        /// <summary>
        /// Modifies the <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The <see cref="string"/> to modify.</param>
        /// <param name="project">The <see cref="SqlProject"/> used as data source for certain modifiers.</param>
        /// <param name="configuration">The <see cref="ConfigurationModel"/> used as data source for certain modifiers.</param>
        /// <param name="paths">The <see cref="PathCollection"/> used as data source for certain modifiers.</param>
        /// <exception cref="ArgumentNullException"><paramref name="input"/>, <paramref name="project"/> or <paramref name="configuration"/> are <b>null</b>.</exception>
        /// <returns>The modified <see cref="string"/>.</returns>
        Task<string> ModifyAsync([NotNull] string input,
                                 [NotNull] SqlProject project,
                                 [NotNull] ConfigurationModel configuration,
                                 [NotNull] PathCollection paths);
    }
}