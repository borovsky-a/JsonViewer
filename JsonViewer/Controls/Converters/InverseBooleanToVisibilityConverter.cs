using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JsonViewer.Controls.Converters
{
    public sealed class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(!bool.TryParse(value.ToString(), out var boolValue))
            {
                return Visibility.Visible;
            }             
            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
