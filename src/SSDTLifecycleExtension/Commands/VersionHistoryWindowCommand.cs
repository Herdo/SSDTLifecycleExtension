namespace SSDTLifecycleExtension.Commands
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Windows;
    using JetBrains.Annotations;
    using Microsoft.VisualStudio.Shell;
    using Shared.Contracts.DataAccess;
    using Shared.Contracts.Services;
    using ViewModels;

    [UsedImplicitly]
    [ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
    internal sealed class VersionHistoryWindowCommand : WindowBaseCommand<VersionHistoryWindow, VersionHistoryViewModel>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0902;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(Constants.CommandSetGuid);

        public VersionHistoryWindowCommand(SSDTLifecycleExtensionPackage package,
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

        public static VersionHistoryWindowCommand Instance
        {
            get;
            private set;
        }

        public static void Initialize(VersionHistoryWindowCommand instance)
        {
            Instance = instance;
        }
    }
}
