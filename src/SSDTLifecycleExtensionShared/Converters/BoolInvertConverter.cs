namespace SSDTLifecycleExtension.Converters;

public class BoolInvertConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (!(value is bool b))
            throw new ArgumentException($"Must be {nameof(Boolean)}.", nameof(value));
        if (targetType != typeof(bool))
            throw new ArgumentException($"Must be {nameof(Boolean)}.", nameof(targetType));

        return !b;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}