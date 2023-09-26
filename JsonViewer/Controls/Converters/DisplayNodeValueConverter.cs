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
            if (value == null) return value;
            var item = (JsonItem)value;
            if (string.IsNullOrEmpty(item.Value))
            {
                return item.Name;
            }
            return $"{item.Name} : {item.Value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
