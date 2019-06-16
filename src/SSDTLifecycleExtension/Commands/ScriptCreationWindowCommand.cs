namespace SSDTLifecycleExtension.Commands
{
    using System;
    using Windows;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.Shell;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using ViewModels;

    [UsedImplicitly]
    internal sealed class ScriptCreationWindowCommand : WindowBaseCommand<ScriptCreationWindow, ScriptCreationViewModel>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0901;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(Constants.CommandSetGuid);

        public ScriptCreationWindowCommand(SSDTLifecycleExtensionPackage package,
                                           DependencyResolver dependencyResolver,
                                           OleMenuCommandService commandService,
                                           ICommandAvailabilityService commandAvailabilityService,
                                           IVisualStudioAccess visualStudioAccess)
            : base(package,
                   dependencyResolver,
                   commandService,
                   commandAvailabilityService,
                   visualStudioAccess,
                   CommandId,
                   CommandSet)
        {
        }

        public static ScriptCreationWindowCommand Instance
        {
            get;
            private set;
        }

        public static void Initialize(ScriptCreationWindowCommand instance)
        {
            Instance = instance;
        }
    }
}
