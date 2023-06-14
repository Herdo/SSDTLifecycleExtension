namespace SSDTLifecycleExtension.OutputClassification;

[ContentType("output")]
[Export(typeof(IClassifierProvider))]
public class OutputClassifierProvider : IClassifierProvider
{
    [Import] internal IClassificationTypeRegistryService ClassificationTypeRegistryService;

    private OutputClassifier _outputClassifier;

    IClassifier IClassifierProvider.GetClassifier(ITextBuffer textBuffer)
    {
        return _outputClassifier ?? (_outputClassifier = new OutputClassifier(ClassificationTypeRegistryService));
    }
}