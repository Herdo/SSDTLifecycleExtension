namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Text;
    using Contracts;
    using Models;

    internal class AddCustomHeaderModifier : IScriptModifier
    {
        string IScriptModifier.Modify(string input,
                                      SqlProject project,
                                      ConfigurationModel configuration,
                                      ScriptCreationVariables variables)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (project == null)
                throw new ArgumentNullException(nameof(project));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(configuration.CustomHeader))
                return input;

            var sb = new StringBuilder(configuration.CustomHeader);
            sb.AppendLine();
            sb.Append(input);
            return sb.ToString();
        }
    }
}