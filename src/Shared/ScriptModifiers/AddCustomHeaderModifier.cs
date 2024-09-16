namespace SSDTLifecycleExtension.Shared.ScriptModifiers;

internal class AddCustomHeaderModifier : IScriptModifier
{
    Task IScriptModifier.ModifyAsync(ScriptModificationModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Configuration.CustomHeader))
            return Task.CompletedTask;

        Guard.IsNotNull(model.Project.ProjectProperties.DacVersion);

        var header = model.Configuration.CustomHeader!;
        header = header.Replace(Constants.ScriptModificationSpecialKeywordPreviousVersion, model.PreviousVersion.ToString());
        header = header.Replace(Constants.ScriptModificationSpecialKeywordNextVersion,
                                model.CreateLatest ? "latest" : model.Project.ProjectProperties.DacVersion.ToString());
        var sb = new StringBuilder(header);
        sb.AppendLine();
        sb.Append(model.CurrentScript);
        model.CurrentScript = sb.ToString();
        return Task.CompletedTask;
    }
}