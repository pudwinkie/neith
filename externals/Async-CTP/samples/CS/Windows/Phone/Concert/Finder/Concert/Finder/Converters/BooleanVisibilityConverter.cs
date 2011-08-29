using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ConcertFinder.Converters
{
    /// <summary>
    /// Converter for boolean values.
    /// </summary>
    public class BooleanVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Convert a boolean value to a visibility.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = Visibility.Visible;

            var originalValue = (bool)value;
            var isInverted = (parameter as string ?? String.Empty).Equals("IsInverted");
            if (originalValue == true)
            {
                convertedValue = !isInverted ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                convertedValue = !isInverted ? Visibility.Collapsed : Visibility.Visible;
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
