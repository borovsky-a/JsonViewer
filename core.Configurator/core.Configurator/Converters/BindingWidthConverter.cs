using System;
using System.Globalization;
using System.Windows.Data;

namespace mop.Configurator.Converters
{
    public class BindingWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;
            double d_value;
            if (!double.TryParse(value.ToString(), out d_value))
                return 0;
            if (d_value < 24)
                return 0;
            return d_value - 24;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(BindingWidthConverter)}.{nameof(ConvertBack)}");
        }
    }
}
