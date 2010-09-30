using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class StringExtensions
    {
        public static string FormatText(this string format, object arg1) { return string.Format(format, arg1); }
        public static string FormatText(this string format, object arg1, object arg2) { return string.Format(format, arg1, arg2); }
        public static string FormatText(this string format, object arg1, object arg2, object arg3) { return string.Format(format, arg1, arg2, arg3); }
        public static string FormatText(this string format, params object[] args) { return string.Format(format, args); }
    }
}
