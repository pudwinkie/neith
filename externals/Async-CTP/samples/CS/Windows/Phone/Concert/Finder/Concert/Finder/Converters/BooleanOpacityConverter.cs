using System;
using System.Globalization;
using System.Windows.Data;

namespace ConcertFinder.Converters
{
    /// <summary>
    /// Converter for boolean values.
    /// </summary>
    public class BooleanOpacityConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean value to an opacity.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = 1;

            var originalValue = (bool)value;
            convertedValue = (originalValue == true) ? 1 : 0;

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
