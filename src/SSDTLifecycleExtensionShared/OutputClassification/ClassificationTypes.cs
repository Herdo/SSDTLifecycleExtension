#nullable enable

namespace SSDTLifecycleExtension.OutputClassification;

public static class ClassificationTypes
{
    public const string Critical = "SsdtLifecycleExtensionCritical";
    public const string Error = "SsdtLifecycleExtensionError";
    public const string Warning = "SsdtLifecycleExtensionWarning";
    public const string Debug = "SsdtLifecycleExtensionDebug";
    public const string Trace = "SsdtLifecycleExtensionTrace";
    public const string Done = "SsdtLifecycleExtensionDone";

    #region Critical

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(Critical)]
    public static ClassificationTypeDefinition? CriticalTypeDefinition { get; set; }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Critical)]
    [Name(Critical)]
    public sealed class CriticalClassification : ClassificationFormatDefinition
    {
        public CriticalClassification()
        {
            ForegroundColor = Colors.Firebrick;
            IsBold = true;
        }
    }

    #endregion

    #region Error

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(Error)]
    public static ClassificationTypeDefinition? ErrorTypeDefinition { get; set; }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Error)]
    [Name(Error)]
    public sealed class ErrorClassification : ClassificationFormatDefinition
    {
        public ErrorClassification()
        {
            ForegroundColor = Colors.Red;
            IsBold = true;
        }
    }

    #endregion

    #region Warning

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(Warning)]
    public static ClassificationTypeDefinition? WarningTypeDefinition { get; set; }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Warning)]
    [Name(Warning)]
    public sealed class WarningClassification : ClassificationFormatDefinition
    {
        public WarningClassification()
        {
            ForegroundColor = Colors.DarkOrange;
            IsBold = true;
        }
    }

    #endregion

    #region Debug

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(Debug)]
    public static ClassificationTypeDefinition? DebugTypeDefinition { get; set; }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Debug)]
    [Name(Debug)]
    public sealed class DebugClassification : ClassificationFormatDefinition
    {
        public DebugClassification()
        {
            ForegroundColor = Colors.Gray;
        }
    }

    #endregion

    #region Trace

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(Trace)]
    public static ClassificationTypeDefinition? TraceTypeDefinition { get; set; }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Trace)]
    [Name(Trace)]
    public sealed class TraceClassification : ClassificationFormatDefinition
    {
        public TraceClassification()
        {
            ForegroundColor = Colors.Gray;
        }
    }

    #endregion

    #region Done

    [Export(typeof(ClassificationTypeDefinition))]
    [Name(Done)]
    public static ClassificationTypeDefinition? DoneTypeDefinition { get; set; }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Done)]
    [Name(Done)]
    public sealed class DoneClassification : ClassificationFormatDefinition
    {
        public DoneClassification()
        {
            ForegroundColor = Colors.Green;
        }
    }

    #endregion
}