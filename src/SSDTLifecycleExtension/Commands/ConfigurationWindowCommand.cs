namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Windows;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.Shell;
    using Shared.Contracts.Services;
    using ViewModels;

    [UsedImplicitly]
    [ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
    internal sealed class ConfigurationWindowCommand : WindowBaseCommand<ConfigurationWindow, ConfigurationViewModel>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0903;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(Constants.CommandSetGuid);

        public ConfigurationWindowCommand(SSDTLifecycleExtensionPackage package,
                                          OleMenuCommandService commandService,
                                          ICommandAvailabilityService commandAvailabilityService,
                                          ToolWindowInitializer toolWindowInitializer)
            : base(package,
                   commandService,
                   commandAvailabilityService,
                   CommandId,
                   CommandSet,
                   toolWindowInitializer)
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
