using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace mop.Configurator.Controls
{
    public class TreeListViewConverter : IValueConverter
    {
        public const double Indentation = 10;

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return null;

            return Indentation * (((DependencyObject)value).VisualAncestors().OfType<TreeViewItem>().Count() - 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(TreeListViewConverter)}.{nameof(ConvertBack)}");
        }
    }
}
