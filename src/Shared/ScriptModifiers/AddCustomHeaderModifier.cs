namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Contracts;
    using Models;

    internal class AddCustomHeaderModifier : IScriptModifier
    {
        Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (string.IsNullOrWhiteSpace(model.Configuration.CustomHeader))
                return Task.CompletedTask;

            var header = model.Configuration.CustomHeader;
            header = header.Replace(Constants.ScriptModificationSpecialKeywordPreviousVersion, model.PreviousVersion.ToString());
            header = header.Replace(Constants.ScriptModificationSpecialKeywordNextVersion, model.CreateLatest ? "latest" : model.Project.ProjectProperties.DacVersion.ToString());
            var sb = new StringBuilder(header);
            sb.AppendLine();
            sb.Append(model.CurrentScript);
            model.CurrentScript = sb.ToString();
            return Task.CompletedTask;
        }
    }
}