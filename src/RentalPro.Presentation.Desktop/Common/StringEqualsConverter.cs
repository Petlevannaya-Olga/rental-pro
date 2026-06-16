using System.Globalization;
using System.Windows.Data;

namespace RentalPro.Presentation.Desktop.Common;

public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true
            ? parameter?.ToString() ?? string.Empty
            : Binding.DoNothing;
    }
}