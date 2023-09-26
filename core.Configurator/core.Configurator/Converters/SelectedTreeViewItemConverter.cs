using System;
using System.Globalization;
using System.Windows.Data;

namespace mop.Configurator.Converters
{
    public class SelectedTreeViewItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object notUsedParameter, CultureInfo culture)
        {
            //if (values == null || values.Length < 2)
            //    return false;

            //var section = values[1] as ParameterSection;
            //var parameter = values[0] as Parameter;
            //if (section == null || parameter == null)
            //    return false;

            //if (section.Selected != parameter)
            //    return false;

            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(SelectedTreeViewItemConverter)}.{nameof(ConvertBack)}");
        }
    }
}
