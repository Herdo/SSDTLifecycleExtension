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
        Assert.AreEqual(1, contentTypeAttributes.Length);
        Assert.AreEqual("output", contentTypeAttributes[0].ContentTypes);
        Assert.AreEqual(1, exportAttributes.Length);
        Assert.AreEqual(typeof(IClassifierProvider), exportAttributes[0].ContractType);
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
        Assert.DoesNotThrow(() => provider.GetClassifier(null));
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
        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreSame(instance1, instance2);
    }
}