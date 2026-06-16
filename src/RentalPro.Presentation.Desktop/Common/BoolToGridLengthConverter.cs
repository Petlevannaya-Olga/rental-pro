using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RentalPro.Presentation.Desktop.Common;

public sealed class BoolToGridLengthConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        return value is true
            ? new GridLength(82)
            : new GridLength(270);
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}