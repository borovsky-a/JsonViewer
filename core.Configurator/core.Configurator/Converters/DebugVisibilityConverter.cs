using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace mop.Configurator.Converters
{
    public class DebugVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if DEBUG
            return Visibility.Visible;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            return Visibility.Collapsed;
#pragma warning restore CS0162 // Unreachable code detected
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(DebugVisibilityConverter)}.{nameof(ConvertBack)}");
        }
    }
}
