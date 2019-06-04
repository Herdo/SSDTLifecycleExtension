namespace SSDTLifecycleExtension.Shared.ScriptModifiers
{
    /// <summary>
    /// Possible script modifiers, as well as their execution order.
    /// </summary>
    public enum ScriptModifier
    {
        // ReSharper disable once UnusedMember.Global
        Undefined = 0,
        TrackDacpacVersion = 1000,
        AddCustomHeader = 2000,
        AddCustomFooter = 3000,
    }
}