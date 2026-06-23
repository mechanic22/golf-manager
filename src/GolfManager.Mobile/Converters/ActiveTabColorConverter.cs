using System.Globalization;

namespace GolfManager.Mobile.Converters;

public class ActiveTabColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Color.FromArgb("#2E7D32") : Color.FromArgb("#1B5E20");

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
