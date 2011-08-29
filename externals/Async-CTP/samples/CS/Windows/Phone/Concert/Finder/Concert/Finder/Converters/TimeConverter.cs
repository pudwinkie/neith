using System;
using System.Globalization;
using System.Windows.Data;

namespace ConcertFinder.Converters
{
    /// <summary>
    /// Converter for DateTime values.
    /// </summary>
    public class TimeConverter : IValueConverter
    {
        /// <summary>
        /// Convert a DateTime value to a string using the given format string parameter.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = String.Empty;

            var originalValue = value as DateTime?;
            var format = parameter as string;
            if (originalValue.HasValue && !String.IsNullOrEmpty(format))
            {
                convertedValue = originalValue.Value.ToString(format);
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
