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
    internal sealed class VersionHistoryWindowCommand : WindowBaseCommand<VersionHistoryWindow, VersionHistoryViewModel>
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public const int CommandId = 0x0902;

        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly Guid CommandSet = new Guid(Constants.CommandSetGuid);

        public VersionHistoryWindowCommand(SSDTLifecycleExtensionPackage package,
                                           OleMenuCommandService commandService,
                                           ICommandAvailabilityService commandAvailabilityService,
                                           IVisualStudioAccess visualStudioAccess)
            : base(package,
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
