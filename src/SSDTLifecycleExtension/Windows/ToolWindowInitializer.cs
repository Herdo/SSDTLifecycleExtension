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
        }

        private async Task<bool> TryInitializeToolWindowInternalAsync<TViewModel>(IVisualStudioToolWindow window) where TViewModel : IViewModel
        {
            // Set caption
            var project = _visualStudioAccess.GetSelectedSqlProject();
            if (project == null)
                return false;
            window.SetCaption(project.Name);

            // Set data context
            if (!(window.Content is IView windowContent))
                return true;

            var viewModel = _dependencyResolver.GetViewModel<TViewModel>(project);
            var initializedSuccessfully = await viewModel.InitializeAsync();
            if (!initializedSuccessfully)
                return false;
            windowContent.SetDataContext(viewModel);

            return true;
        }

        internal Task<bool> TryInitializeToolWindowAsync<TViewModel>([NotNull] IVisualStudioToolWindow window)
            where TViewModel : IViewModel
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            return TryInitializeToolWindowInternalAsync<TViewModel>(window);
        }
    }
}