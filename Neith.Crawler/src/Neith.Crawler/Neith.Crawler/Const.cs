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

        /// <summary>NeithXFN Type情報格納パス</summary>
        public static string NeithXFNTypesDir { get; internal set; }

        /// <summary>NeithXFNのNamespace</summary>
        public const string NSNeith = @"http://neith.vbel.net/ns/2010/";

        /// <summary>FF14のNamespace</summary>
        public const string NS14 = @"http://ff14.vbel.net/ns/2010/";


        static Const()
        {
            DataDir = AssemblyUtil
                .GetCallingAssemblyDirctory()
                .PathConbine("..", "..", "..", "..", "..", "data")
                .ToFullPath();
            CacheDir = DataDir.PathConbine("cache").ToFullPath();
            NeithXFNTypesDir = DataDir.PathConbine("xfn").ToFullPath();
        }
    }
}
