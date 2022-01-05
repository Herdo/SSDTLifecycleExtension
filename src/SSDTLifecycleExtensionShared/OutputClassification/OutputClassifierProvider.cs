namespace SSDTLifecycleExtension.OutputClassification
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Microsoft.VisualStudio.Utilities;

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
}