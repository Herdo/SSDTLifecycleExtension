namespace SSDTLifecycleExtension.Commands;

[UsedImplicitly]
[ExcludeFromCodeCoverage] // Test would require a Visual Studio shell.
internal sealed class ScriptCreationWindowCommand : WindowBaseCommand<ScriptCreationWindow, ScriptCreationViewModel>
{
    // ReSharper disable once MemberCanBePrivate.Global
    public const int CommandId = 0x0901;

    // ReSharper disable once MemberCanBePrivate.Global
    public static readonly Guid CommandSet = new(Constants.CommandSetGuid);

    public ScriptCreationWindowCommand(SSDTLifecycleExtensionPackage package,
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