namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Text;
    using Models;
    using Variables;

    internal class AddCustomFooterModifier : IScriptModifier
    {
        string IScriptModifier.Modify(string input,
                                      ConfigurationModel configuration,
                                      ScriptCreationVariables variables)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (string.IsNullOrWhiteSpace(configuration.CustomFooter))
                return input;

            var sb = new StringBuilder(input);
            sb.AppendLine();
            sb.Append(configuration.CustomFooter);
            return sb.ToString();
        }
    }
}