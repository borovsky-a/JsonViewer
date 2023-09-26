using mop.Configurator.Log;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace mop.Configurator.Converters
{
    public class LogLevelForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (LogLevel)value;
            switch (v)
            {
                case LogLevel.None:
                case LogLevel.Trace:
                case LogLevel.Debug:
                    if (parameter != null)
                    {
                        return new SolidColorBrush(Colors.Black);
                    }
                    return new SolidColorBrush(Color.FromRgb(47, 47, 47));
                case LogLevel.Information:
                    return new SolidColorBrush(Color.FromRgb(51, 122, 183));
                case LogLevel.Warning:
                    return new SolidColorBrush(Color.FromRgb(138, 109, 59));
                case LogLevel.Error:
                case LogLevel.Critical:
                    return new SolidColorBrush(Color.FromRgb(169, 68, 66));
                case LogLevel.Success:
                    return new SolidColorBrush(Color.FromRgb(60, 118, 61));
                default:
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(LogLevelForegroundConverter)}.{nameof(ConvertBack)}");
        }
    }
}
