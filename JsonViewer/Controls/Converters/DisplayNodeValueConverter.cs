using JsonViewer.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace JsonViewer.Controls.Converters
{
    public sealed class DisplayNodeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            var item = (JsonItem)value;            
            return item.GetDisplayValue();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
