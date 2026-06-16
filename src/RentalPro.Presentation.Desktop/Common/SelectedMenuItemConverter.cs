using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RentalPro.Presentation.Desktop.Common;

public sealed class SelectedMenuItemConverter : IMultiValueConverter
{
    public object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (values.Length < 2)
            return false;

        if (values[0] == DependencyProperty.UnsetValue ||
            values[1] == DependencyProperty.UnsetValue)
            return false;

        var currentPage = values[0]?.ToString();
        var menuPage = values[1]?.ToString();

        if (string.IsNullOrWhiteSpace(currentPage) ||
            string.IsNullOrWhiteSpace(menuPage))
            return false;

        return currentPage == menuPage;
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}