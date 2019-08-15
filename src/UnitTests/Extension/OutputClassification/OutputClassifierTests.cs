using NUnit.Framework;

namespace SSDTLifecycleExtension.UnitTests.Extension.OutputClassification
{
    using System;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Classification;
    using Moq;
    using SSDTLifecycleExtension.OutputClassification;

    [TestFixture]
    public class OutputClassifierTests
    {
        [Test]
        public void Constructor_ArgumentNullException_ClassificationTypeRegistryService()
        {
            // Act & Assert
            // ReSharper disable once ObjectCreationAsStatement
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => new OutputClassifier(null));
        }

        [Test]
        public void ClassificationChanged_NotSupportedException()
        {
            // Arrange
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs e)
            {
                Assert.Fail("This shouldn't be called.");
            }

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => classifier.ClassificationChanged += ClassifierClassificationChanged);
            Assert.Throws<NotSupportedException>(() => classifier.ClassificationChanged += ClassifierClassificationChanged);
        }

        [Test]
        public void GetClassificationSpans_EmptyWhenNoSnapshot()
        {
            // Arrange
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            var span = new SnapshotSpan();

            // Act
            var spans = classifier.GetClassificationSpans(span);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(0, spans.Count);
        }

        [Test]
        public void GetClassificationSpans_EmptyWhenEmptySnapshot()
        {
            // Arrange
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.SetupGet(m => m.Length).Returns(0);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, new Span());

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(0, spans.Count);
        }

        [Test]
        public void GetClassificationSpans_EmptyWhenNonMatchingSnapshot()
        {
            // Arrange
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "foobar";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(0, spans.Count);
        }

        [Test]
        public void GetClassificationSpans_ClassifyAsCritical()
        {
            // Arrange
            var criticalClassificationType = Mock.Of<IClassificationType>();
            var errorClassificationType = Mock.Of<IClassificationType>();
            var warningClassificationType = Mock.Of<IClassificationType>();
            var debugClassificationType = Mock.Of<IClassificationType>();
            var traceClassificationType = Mock.Of<IClassificationType>();
            var doneClassificationType = Mock.Of<IClassificationType>();
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Critical)).Returns(criticalClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Error)).Returns(errorClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Warning)).Returns(warningClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Debug)).Returns(debugClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Trace)).Returns(traceClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Done)).Returns(doneClassificationType);
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "CRITICAL: foobar";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Count);
            Assert.AreEqual(snapshotSpan, spans[0].Span);
            classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
            Assert.AreSame(criticalClassificationType, spans[0].ClassificationType);
        }

        [Test]
        public void GetClassificationSpans_ClassifyAsError()
        {
            // Arrange
            var criticalClassificationType = Mock.Of<IClassificationType>();
            var errorClassificationType = Mock.Of<IClassificationType>();
            var warningClassificationType = Mock.Of<IClassificationType>();
            var debugClassificationType = Mock.Of<IClassificationType>();
            var traceClassificationType = Mock.Of<IClassificationType>();
            var doneClassificationType = Mock.Of<IClassificationType>();
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Critical)).Returns(criticalClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Error)).Returns(errorClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Warning)).Returns(warningClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Debug)).Returns(debugClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Trace)).Returns(traceClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Done)).Returns(doneClassificationType);
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "ERROR: foobar";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Count);
            Assert.AreEqual(snapshotSpan, spans[0].Span);
            classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
            Assert.AreSame(errorClassificationType, spans[0].ClassificationType);
        }

        [Test]
        public void GetClassificationSpans_ClassifyAsWarning()
        {
            // Arrange
            var criticalClassificationType = Mock.Of<IClassificationType>();
            var errorClassificationType = Mock.Of<IClassificationType>();
            var warningClassificationType = Mock.Of<IClassificationType>();
            var debugClassificationType = Mock.Of<IClassificationType>();
            var traceClassificationType = Mock.Of<IClassificationType>();
            var doneClassificationType = Mock.Of<IClassificationType>();
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Critical)).Returns(criticalClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Error)).Returns(errorClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Warning)).Returns(warningClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Debug)).Returns(debugClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Trace)).Returns(traceClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Done)).Returns(doneClassificationType);
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "WARNING: foobar";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Count);
            Assert.AreEqual(snapshotSpan, spans[0].Span);
            classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
            Assert.AreSame(warningClassificationType, spans[0].ClassificationType);
        }

        [Test]
        public void GetClassificationSpans_ClassifyAsDebug()
        {
            // Arrange
            var criticalClassificationType = Mock.Of<IClassificationType>();
            var errorClassificationType = Mock.Of<IClassificationType>();
            var warningClassificationType = Mock.Of<IClassificationType>();
            var debugClassificationType = Mock.Of<IClassificationType>();
            var traceClassificationType = Mock.Of<IClassificationType>();
            var doneClassificationType = Mock.Of<IClassificationType>();
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Critical)).Returns(criticalClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Error)).Returns(errorClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Warning)).Returns(warningClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Debug)).Returns(debugClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Trace)).Returns(traceClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Done)).Returns(doneClassificationType);
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "DEBUG: foobar";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Count);
            Assert.AreEqual(snapshotSpan, spans[0].Span);
            classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
            Assert.AreSame(debugClassificationType, spans[0].ClassificationType);
        }

        [Test]
        public void GetClassificationSpans_ClassifyAsTrace()
        {
            // Arrange
            var criticalClassificationType = Mock.Of<IClassificationType>();
            var errorClassificationType = Mock.Of<IClassificationType>();
            var warningClassificationType = Mock.Of<IClassificationType>();
            var debugClassificationType = Mock.Of<IClassificationType>();
            var traceClassificationType = Mock.Of<IClassificationType>();
            var doneClassificationType = Mock.Of<IClassificationType>();
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Critical)).Returns(criticalClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Error)).Returns(errorClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Warning)).Returns(warningClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Debug)).Returns(debugClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Trace)).Returns(traceClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Done)).Returns(doneClassificationType);
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "TRACE: foobar";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Count);
            Assert.AreEqual(snapshotSpan, spans[0].Span);
            classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
            Assert.AreSame(traceClassificationType, spans[0].ClassificationType);
        }

        [Test]
        public void GetClassificationSpans_ClassifyAsDone()
        {
            // Arrange
            var criticalClassificationType = Mock.Of<IClassificationType>();
            var errorClassificationType = Mock.Of<IClassificationType>();
            var warningClassificationType = Mock.Of<IClassificationType>();
            var debugClassificationType = Mock.Of<IClassificationType>();
            var traceClassificationType = Mock.Of<IClassificationType>();
            var doneClassificationType = Mock.Of<IClassificationType>();
            var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Critical)).Returns(criticalClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Error)).Returns(errorClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Warning)).Returns(warningClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Debug)).Returns(debugClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Trace)).Returns(traceClassificationType);
            classificationTypeRegistryServiceMock.Setup(m => m.GetClassificationType(ClassificationTypes.Done)).Returns(doneClassificationType);
            IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
            const string text = "INFO: ========== foobar ==========";
            var span = new Span(0, text.Length);
            var snapshotMock = new Mock<ITextSnapshot>();
            snapshotMock.Setup(m => m.GetText(span)).Returns(text);
            snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
            var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

            // Act
            var spans = classifier.GetClassificationSpans(snapshotSpan);

            // Assert
            Assert.IsNotNull(spans);
            Assert.AreEqual(1, spans.Count);
            Assert.AreEqual(snapshotSpan, spans[0].Span);
            classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
            Assert.AreSame(doneClassificationType, spans[0].ClassificationType);
        }
    }
}