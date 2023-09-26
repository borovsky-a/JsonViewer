using JsonViewer.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace JsonViewer.Controls.Converters
{
    public class TreeViewItemBgValueConverter : IValueConverter
    {
        private const string DEFAULT_COLOR = "#ffffff";
        private const string SELECTED_COLOR = "#33B2FF";
        private const string MATCH_COLOR = "#FFFDC9";
        private const string MATCH_SELECTED_COLOR = "#FFF600";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DEFAULT_COLOR;
            }
            var item = (JsonItem)value;
            if(!item.IsSelected && !item.IsMatch)
            {
                return DEFAULT_COLOR;
            }
            if(item.IsSelected && !item.IsMatch)
            {
                return SELECTED_COLOR;
            }
            if (!item.IsSelected && item.IsMatch)
            {
                 return MATCH_COLOR;
            }
            return MATCH_SELECTED_COLOR;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
