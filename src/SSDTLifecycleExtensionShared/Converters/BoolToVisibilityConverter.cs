namespace SSDTLifecycleExtension.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        if (!(value is bool visible))
            throw new ArgumentException($"Must be {nameof(Boolean)}.", nameof(value));
        if (targetType != typeof(Visibility))
            throw new ArgumentException($"Must be {nameof(Visibility)}.", nameof(targetType));

        var p = parameter?.ToString();
        var invert = p == "invert";
        if (invert)
            visible = !visible;
        return visible
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}