namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Models;

    internal class AddCustomHeaderModifier : IScriptModifier
    {
        Task<string> IScriptModifier.ModifyAsync(string input,
                                                 SqlProject project,
                                                 ConfigurationModel configuration,
                                                 PathCollection paths)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (paths == null)
                throw new ArgumentNullException(nameof(paths));

            if (string.IsNullOrWhiteSpace(configuration.CustomHeader))
                return Task.FromResult(input);

            var sb = new StringBuilder(configuration.CustomHeader);
            sb.AppendLine();
            sb.Append(input);
            return Task.FromResult(sb.ToString());
        }
    }
}