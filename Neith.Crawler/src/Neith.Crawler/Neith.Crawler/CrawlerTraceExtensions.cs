using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace System
{
    internal static class CrawlerTraceExtensions
    {
        static CrawlerTraceExtensions()
        {




        }

        internal static void TraceInfo(this string message)
        {
            Trace.TraceInformation(message);
        }

        internal static void TraceWarning(this string message)
        {
            Trace.TraceWarning(message);
        }

        internal static void TraceError(this string message)
        {
            Trace.TraceError(message);
        }

    }
}
