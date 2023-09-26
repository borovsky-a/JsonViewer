using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace mop.Configurator.Converters
{
#pragma warning disable CS1591
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool incoming;
            if (value == null || !bool.TryParse(value.ToString(), out incoming))
                throw new InvalidOperationException("Параметр должен быть логическим значением.");

            return incoming == false ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
#pragma warning restore CS1591
}
