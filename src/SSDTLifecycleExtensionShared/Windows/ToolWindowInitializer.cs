namespace SSDTLifecycleExtension.Windows
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.Contracts.DataAccess;
    using ViewModels;

    internal class ToolWindowInitializer
    {
        private readonly IVisualStudioAccess _visualStudioAccess;
        [NotNull] private readonly DependencyResolver _dependencyResolver;

        public ToolWindowInitializer([NotNull] IVisualStudioAccess visualStudioAccess,
                                     [NotNull] DependencyResolver dependencyResolver)
        {
            _visualStudioAccess = visualStudioAccess ?? throw new ArgumentNullException(nameof(visualStudioAccess));
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
            _visualStudioAccess.SolutionClosed += (sender, args) => _dependencyResolver.HandleSolutionClosed();
        }

        private async Task<(bool Success, string FullProjectPath)> TryInitializeToolWindowInternalAsync<TViewModel>(IVisualStudioToolWindow window) where TViewModel : IViewModel
        {
            // Set caption
            var project = _visualStudioAccess.GetSelectedSqlProject();
            if (project == null)
                return (false, null);
            window.SetCaption(project.Name);

            // Set data context
            if (!(window.Content is IView windowContent))
                return (true, project.FullName);

            var viewModel = _dependencyResolver.GetViewModel<TViewModel>(project);
            var initializedSuccessfully = await viewModel.InitializeAsync();
            if (!initializedSuccessfully)
                return (false, project.FullName);
            windowContent.SetDataContext(viewModel);

            return (true, project.FullName);
        }

        internal Task<(bool Success, string FullProjectPath)> TryInitializeToolWindowAsync<TViewModel>([NotNull] IVisualStudioToolWindow window)
            where TViewModel : IViewModel
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return TryInitializeToolWindowInternalAsync<TViewModel>(window);
        }
    }
}