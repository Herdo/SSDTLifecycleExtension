namespace SSDTLifecycleExtension.UnitTests.Extension.OutputClassification;

[TestFixture]
public class OutputClassifierProviderTests
{
    [Test]
    public void CorrectAttributes()
    {
        // Arrange
        var t = typeof(OutputClassifierProvider);

        // Act
        var attributes = t.GetCustomAttributes(false);
        var contentTypeAttributes = attributes.OfType<Microsoft.VisualStudio.Utilities.ContentTypeAttribute>().ToArray();
        var exportAttributes = attributes.OfType<System.ComponentModel.Composition.ExportAttribute>().ToArray();

        // Assert
        contentTypeAttributes.Should().ContainSingle()
            .Which.ContentTypes.Should().Be("output");
        exportAttributes.Should().ContainSingle()
            .Which.ContractType.Should().Be(typeof(IClassifierProvider));
    }

    [Test]
    public void GetClassifier_DoesNotThrowWhenTextBufferIsNull()
    {
        // Arrange
        var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
        IClassifierProvider provider = new OutputClassifierProvider
        {
            ClassificationTypeRegistryService = classificationTypeRegistryServiceMock.Object
        };

        // Act & Assert
        provider.Invoking(p => p.GetClassifier(null)).Should().NotThrow();
    }

    [Test]
    public void GetClassifier_ReturnSameInstance()
    {
        // Arrange
        var classificationTypeRegistryServiceMock = new Mock<IClassificationTypeRegistryService>();
        IClassifierProvider provider = new OutputClassifierProvider
        {
            ClassificationTypeRegistryService = classificationTypeRegistryServiceMock.Object
        };

        // Act
        var instance1 = provider.GetClassifier(null);
        var instance2 = provider.GetClassifier(null);

        // Assert
        instance1.Should().NotBeNull();
        instance2.Should().NotBeNull();
        instance1.Should().BeSameAs(instance2);
    }
}
