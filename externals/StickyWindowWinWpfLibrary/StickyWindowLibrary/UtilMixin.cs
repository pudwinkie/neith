using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StickyWindowLibrary
{
    internal static class UtilMixin
    {
        public static bool IsNormal(this double value)
        {
            return !double.IsNaN(value)
                && !double.IsInfinity(value);
        }
    }
}
