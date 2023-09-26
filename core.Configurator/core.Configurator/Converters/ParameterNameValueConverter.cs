using System;
using System.Globalization;
using System.Windows.Data;

namespace mop.Configurator.Converters
{
    public class ParameterNameValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var name = values[0] == null ? "" : values[0].ToString();
            var value = values[1];
            if (value == null)
                return name;
            return name + " :";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(ParameterNameValueConverter)}.{nameof(ConvertBack)}");
        }
    }
}
