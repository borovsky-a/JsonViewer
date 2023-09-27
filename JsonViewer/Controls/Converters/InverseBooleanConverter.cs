using System;
using System.Globalization;
using System.Windows.Data;

namespace JsonViewer.Controls.Converters
{
    public sealed class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!bool.TryParse(value.ToString(), out var boolValue))
            {
                return true;
            }             
            return !boolValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
