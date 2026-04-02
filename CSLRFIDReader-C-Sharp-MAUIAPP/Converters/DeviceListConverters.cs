using Microsoft.Maui.Graphics;
using System.Globalization;

namespace CSLHandheldReader_C_Sharp_MAUIAPP.Converters
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && !b;
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is bool b && b
                ? Color.FromArgb("#2196F3")  // Selected — blue
                : Color.FromArgb("#666666"); // Not selected — gray
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
