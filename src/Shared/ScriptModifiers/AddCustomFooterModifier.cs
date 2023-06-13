namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

internal class AddCustomFooterModifier : IScriptModifier
{
    Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        if (string.IsNullOrWhiteSpace(model.Configuration.CustomFooter))
            return Task.CompletedTask;

        var footer = model.Configuration.CustomFooter;
        footer = footer.Replace(Constants.ScriptModificationSpecialKeywordPreviousVersion, model.PreviousVersion.ToString());
        footer = footer.Replace(Constants.ScriptModificationSpecialKeywordNextVersion,
                                model.CreateLatest ? "latest" : model.Project.ProjectProperties.DacVersion.ToString());
        var sb = new StringBuilder(model.CurrentScript);
        sb.AppendLine();
        sb.Append(footer);
        model.CurrentScript = sb.ToString();
        return Task.CompletedTask;
    }
}