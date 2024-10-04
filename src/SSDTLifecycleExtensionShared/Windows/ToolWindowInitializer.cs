#nullable enable

namespace SSDTLifecycleExtension.Windows;

internal class ToolWindowInitializer
{
    private readonly IVisualStudioAccess _visualStudioAccess;
    private readonly DependencyResolver _dependencyResolver;

    public ToolWindowInitializer(IVisualStudioAccess visualStudioAccess,
        DependencyResolver dependencyResolver)
    {
        _visualStudioAccess = visualStudioAccess;
        _dependencyResolver = dependencyResolver;
        _visualStudioAccess.SolutionClosed += (sender, args) => _dependencyResolver.HandleSolutionClosed();
    }

    private async Task<(bool Success, string? FullProjectPath)> TryInitializeToolWindowInternalAsync<TViewModel>(IVisualStudioToolWindow window) where TViewModel : IViewModel
    {
        // Set caption
        var project = await _visualStudioAccess.GetSelectedSqlProjectAsync();
        if (project is null)
            return (false, null);
        window.SetCaption(project.Name);

        // Set data context
        if (window.Content is not IView windowContent)
            return (true, project.FullName);

        var viewModel = _dependencyResolver.GetViewModel<TViewModel>(project);
        var initializedSuccessfully = await viewModel.InitializeAsync();
        if (!initializedSuccessfully)
            return (false, project.FullName);
        windowContent.SetDataContext(viewModel);

        return (true, project.FullName);
    }

    internal Task<(bool Success, string? FullProjectPath)> TryInitializeToolWindowAsync<TViewModel>(IVisualStudioToolWindow window)
        where TViewModel : IViewModel
    {
        return TryInitializeToolWindowInternalAsync<TViewModel>(window);
    }
}