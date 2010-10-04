using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace System.IO
{
    public static class IOExtensions
    {
        public static string PathConbine(this string path, IEnumerable<string> appends)
        {
            return PathConbineImpl(path, appends);
        }
        public static string PathConbine(this string path, params string[] appends)
        {
            return PathConbineImpl(path, appends);
        }
        private static string PathConbineImpl(this string path, IEnumerable<string> appends)
        {
            foreach (var item in appends) path = Path.Combine(path, item);
            return path;
        }


        public static string ToFullPath(this string path)
        {
            return Path.GetFullPath(path);
        }

        public static string ToDirectoryName(this string path)
        {
            return Path.GetDirectoryName(path);
        }

    }
}
