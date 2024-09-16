#nullable enable

namespace SSDTLifecycleExtension.OutputClassification;

internal sealed class OutputClassifier : IClassifier
{
    private readonly IClassificationTypeRegistryService _classificationTypeRegistryService;

    internal OutputClassifier(IClassificationTypeRegistryService classificationTypeRegistryService)
    {
        _classificationTypeRegistryService = classificationTypeRegistryService;
    }

    IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
    {
        var snapshot = span.Snapshot;
        var spans = new List<ClassificationSpan>();
        if (snapshot == null || snapshot.Length == 0)
            return spans;

        var text = span.GetText();
        if (text.StartsWith("CRITICAL:"))
        {
            var classificationType = _classificationTypeRegistryService.GetClassificationType(ClassificationTypes.Critical);
            spans.Add(new ClassificationSpan(span, classificationType));
        }

        if (text.StartsWith("ERROR:"))
        {
            var classificationType = _classificationTypeRegistryService.GetClassificationType(ClassificationTypes.Error);
            spans.Add(new ClassificationSpan(span, classificationType));
        }

        if (text.StartsWith("WARNING:"))
        {
            var classificationType = _classificationTypeRegistryService.GetClassificationType(ClassificationTypes.Warning);
            spans.Add(new ClassificationSpan(span, classificationType));
        }

        if (text.StartsWith("DEBUG:"))
        {
            var classificationType = _classificationTypeRegistryService.GetClassificationType(ClassificationTypes.Debug);
            spans.Add(new ClassificationSpan(span, classificationType));
        }

        if (text.StartsWith("TRACE:"))
        {
            var classificationType = _classificationTypeRegistryService.GetClassificationType(ClassificationTypes.Trace);
            spans.Add(new ClassificationSpan(span, classificationType));
        }

        if (text.StartsWith("INFO:") && text.Count(c => c == '=') == 20)
        {
            var classificationType = _classificationTypeRegistryService.GetClassificationType(ClassificationTypes.Done);
            spans.Add(new ClassificationSpan(span, classificationType));
        }

        return spans;
    }

    public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;
}