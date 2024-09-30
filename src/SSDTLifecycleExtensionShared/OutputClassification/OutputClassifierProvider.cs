#nullable enable

namespace SSDTLifecycleExtension.OutputClassification;

[ContentType("output")]
[Export(typeof(IClassifierProvider))]
public class OutputClassifierProvider : IClassifierProvider
{
    [Import] internal IClassificationTypeRegistryService ClassificationTypeRegistryService = default!;

    private OutputClassifier? _outputClassifier;

    IClassifier IClassifierProvider.GetClassifier(ITextBuffer textBuffer)
    {
        return _outputClassifier ??= new OutputClassifier(ClassificationTypeRegistryService);
    }
}