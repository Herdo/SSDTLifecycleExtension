namespace SSDTLifecycleExtension.UnitTests.Extension.OutputClassification;

[TestFixture]
public class OutputClassifierTests
{
    [Test]
    public void GetClassificationSpans_EmptyWhenNoSnapshot()
    {
        // Arrange
        var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
        IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        var span = new SnapshotSpan();

        // Act
        var spans = classifier.GetClassificationSpans(span);

        // Assert
        spans.Should().BeEmpty();
    }

    [Test]
    public void GetClassificationSpans_EmptyWhenEmptySnapshot()
    {
        // Arrange
        var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
        IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.SetupGet(m => m.Length).Returns(0);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, new Span());

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().BeEmpty();
    }

    [Test]
    public void GetClassificationSpans_EmptyWhenNonMatchingSnapshot()
    {
        // Arrange
        var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
        IClassifier classifier = new OutputClassifier(classificationTypeRegistryServiceMock.Object);
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "foobar";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().BeEmpty();
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
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "CRITICAL: foobar";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().ContainSingle()
            .Which.Span.Should().Be(snapshotSpan);
        spans.Should().ContainSingle()
            .Which.ClassificationType.Should().Be(criticalClassificationType);
        classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
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
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "ERROR: foobar";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().ContainSingle()
            .Which.Span.Should().Be(snapshotSpan);
        spans.Should().ContainSingle()
            .Which.ClassificationType.Should().Be(errorClassificationType);
        classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
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
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "WARNING: foobar";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().ContainSingle()
            .Which.Span.Should().Be(snapshotSpan);
        spans.Should().ContainSingle()
            .Which.ClassificationType.Should().Be(warningClassificationType);
        classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
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
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "DEBUG: foobar";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().ContainSingle()
            .Which.Span.Should().Be(snapshotSpan);
        spans.Should().ContainSingle()
            .Which.ClassificationType.Should().Be(debugClassificationType);
        classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
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
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "TRACE: foobar";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().ContainSingle()
            .Which.Span.Should().Be(snapshotSpan);
        spans.Should().ContainSingle()
            .Which.ClassificationType.Should().Be(traceClassificationType);
        classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
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
        void ClassifierClassificationChanged(object sender, ClassificationChangedEventArgs args) => Assert.Fail("This shouldn't be invoked.");
        classifier.ClassificationChanged -= ClassifierClassificationChanged;
        classifier.ClassificationChanged += ClassifierClassificationChanged;
        const string text = "INFO: ========== foobar ==========";
        var span = new Span(0, text.Length);
        var snapshotMock = new Mock<ITextSnapshot>();
        snapshotMock.Setup(m => m.GetText(span)).Returns(text);
        snapshotMock.SetupGet(m => m.Length).Returns(text.Length);
        var snapshotSpan = new SnapshotSpan(snapshotMock.Object, span);

        // Act
        var spans = classifier.GetClassificationSpans(snapshotSpan);

        // Assert
        spans.Should().ContainSingle()
            .Which.Span.Should().Be(snapshotSpan);
        spans.Should().ContainSingle()
            .Which.ClassificationType.Should().Be(doneClassificationType);
        classificationTypeRegistryServiceMock.Verify(m => m.GetClassificationType(It.IsAny<string>()), Times.Once);
    }
}