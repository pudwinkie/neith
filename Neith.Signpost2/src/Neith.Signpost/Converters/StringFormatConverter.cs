using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace Neith.Signpost.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string)) throw new ArgumentException("string型への変換以外はサポートしません");
            var format = parameter as string;
            if (format == null) throw new ArgumentException("パラメータがstring.Format引数ではありません");
            format = "{0:" + format + "}";
            return string.Format(format, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
