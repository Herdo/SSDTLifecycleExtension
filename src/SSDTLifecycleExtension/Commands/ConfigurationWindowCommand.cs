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
    internal sealed class ConfigurationWindowCommand : WindowBaseCommand<ConfigurationWindow, ConfigurationViewModel>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0903;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(Constants.CommandSetGuid);

        public ConfigurationWindowCommand(SSDTLifecycleExtensionPackage package,
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

        public static ConfigurationWindowCommand Instance
        {
            get;
            private set;
        }

        public static void Initialize(ConfigurationWindowCommand instance)
        {
            Instance = instance;
        }
    }
}
