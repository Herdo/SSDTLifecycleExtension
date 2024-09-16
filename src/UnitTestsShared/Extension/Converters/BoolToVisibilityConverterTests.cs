namespace SSDTLifecycleExtension.UnitTests.Extension.Converters;

[TestFixture]
public class BoolToVisibilityConverterTests
{
    [Test]
    public void Convert_ArgumentNullException_Value()
    {
        // Arrange
        IValueConverter converter = new BoolToVisibilityConverter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void Convert_ArgumentException_InvalidValueType()
    {
        // Arrange
        IValueConverter converter = new BoolToVisibilityConverter();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => converter.Convert(1, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Test]
    public void Convert_ArgumentException_InvalidTargetType()
    {
        // Arrange
        IValueConverter converter = new BoolToVisibilityConverter();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => converter.Convert(true, typeof(long), null, CultureInfo.InvariantCulture));
    }

    [Test]
    [TestCase(true, null, Visibility.Visible)]
    [TestCase(true, "invert", Visibility.Collapsed)]
    [TestCase(false, null, Visibility.Collapsed)]
    [TestCase(false, "invert", Visibility.Visible)]
    public void Convert_CorrectConversion(bool input, object parameter, Visibility expected)
    {
        // Arrange
        IValueConverter converter = new BoolToVisibilityConverter();

        // Act
        var converted = converter.Convert(input, typeof(Visibility), parameter, CultureInfo.InvariantCulture);

        // Assert
        converted.Should().Be(expected);
    }

    [Test]
    public void ConvertBack_NotSupportedException()
    {
        // Arrange
        IValueConverter converter = new BoolToVisibilityConverter();

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => converter.ConvertBack(true, typeof(Visibility), null, CultureInfo.InvariantCulture));
    }
}