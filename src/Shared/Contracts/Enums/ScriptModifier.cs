namespace SSDTLifecycleExtension.Shared.Contracts.Enums;

/// <summary>
///     Possible script modifiers, as well as their execution order.
/// </summary>
public enum ScriptModifier
{
    // ReSharper disable once UnusedMember.Global
    Undefined = 0,
    CommentOutUnnamedDefaultConstraintDrops = 500,
    ReplaceUnnamedDefaultConstraintDrops = 501,
    TrackDacpacVersion = 1000,
    AddCustomHeader = 2000,
    AddCustomFooter = 3000,
    RemoveSqlCmdStatements = 2_147_483_647
}