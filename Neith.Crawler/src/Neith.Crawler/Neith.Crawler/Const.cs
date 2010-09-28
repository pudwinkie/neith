using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Neith.Crawler.Reflection;

namespace Neith.Crawler
{
    /// <summary>
    /// 定数定義
    /// </summary>
    public static class Const
    {
        /// <summary>Data格納パス</summary>
        public static string DataDir { get; internal set; }

        /// <summary>Cache格納パス</summary>
        public static string CacheDir { get; internal set; }


        static Const()
        {
            DataDir = AssemblyUtil
                .GetCallingAssemblyDirctory()
                .PathConbine("..", "..", "..", "..", "..", "data")
                .ToFullPath();
            CacheDir = DataDir.PathConbine("cache").ToFullPath();
        }
    }
}
