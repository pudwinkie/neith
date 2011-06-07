using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace Neith.Signpost.Converters
{
    public class ToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Visibility)) throw new ArgumentException("Visibility型への変換以外はサポートしません");
            if (value is bool) {
                var flag = (bool)value;
                return flag ? Visibility.Visible : Visibility.Hidden;
            }
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
