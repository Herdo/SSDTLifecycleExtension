namespace SSDTLifecycleExtension.UnitTests.Extension.Converters;

[TestFixture]
public class BoolInvertConverterTests
{
    [Test]
    public void Convert_ArgumentNullException_Value()
    {
        // Arrange
        IValueConverter converter = new BoolInvertConverter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Convert(null, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void Convert_ArgumentException_InvalidValueType()
    {
        // Arrange
        IValueConverter converter = new BoolInvertConverter();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => converter.Convert(1, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void Convert_ArgumentException_InvalidTargetType()
    {
        // Arrange
        IValueConverter converter = new BoolInvertConverter();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => converter.Convert(true, typeof(long), null, CultureInfo.InvariantCulture));
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void Convert_CorrectConversion(bool input, bool expected)
    {
        // Arrange
        IValueConverter converter = new BoolInvertConverter();

        // Act
        var converted = converter.Convert(input, typeof(bool), null, CultureInfo.InvariantCulture);

        // Assert
        converted.Should().Be(expected);
    }

    [Test]
    public void ConvertBack_NotSupportedException()
    {
        // Arrange
        IValueConverter converter = new BoolInvertConverter();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture));
    }
}