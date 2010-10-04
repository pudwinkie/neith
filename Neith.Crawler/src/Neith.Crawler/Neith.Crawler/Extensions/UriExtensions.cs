using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web
{
    public static class UriExtensions
    {
        public static string ToHtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text);
        }
    }
}
