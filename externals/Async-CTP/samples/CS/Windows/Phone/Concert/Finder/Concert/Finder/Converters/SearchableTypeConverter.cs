using System;
using System.Globalization;
using System.Windows.Data;
using ConcertFinder.Model;
using ConcertFinder.Resources;

namespace ConcertFinder.Converters
{
    /// <summary>
    /// Converter for ISearchable values.
    /// </summary>
    public class SearchableTypeConverter : IValueConverter
    {
        /// <summary>
        /// Convert an ISearchable to a string based on its type.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = String.Empty;

            if (value is Artist)
            {
                convertedValue = Strings.Artist;
            }
            else if (value is Venue)
            {
                convertedValue = Strings.Venue;
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
