using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ConcertFinder.Converters
{
    /// <summary>
    /// Converter for Image paths.
    /// </summary>
    public class ImageConverter : IValueConverter
    {
        /// <summary>
        /// Convert an image name to a path based on the visible theme.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var convertedValue = String.Empty;

            var imageName = parameter as string;
            if (!String.IsNullOrEmpty(imageName))
            {
                convertedValue = (Visibility)App.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible ?
                    String.Format("/Images/Dark/{0}.png", imageName) :
                    String.Format("/Images/Light/{0}.png", imageName);
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
