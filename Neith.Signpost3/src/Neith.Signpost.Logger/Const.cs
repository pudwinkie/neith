using System;
using System.IO;
using Neith.Util.Reflection;
using System.ComponentModel.Composition.Hosting;

namespace Neith.Signpost.Logger
{
    public static class Const
    {
        /// <summary>MEFコンポーザ</summary>
        public static CompositionContainer MEFContainer { get { return _MEFContainer.Value; } }
        private static readonly Lazy<CompositionContainer> _MEFContainer = new Lazy<CompositionContainer>(MEF.Compose);


        public static readonly string AssemblyUtilDirPath = AssemblyUtil.GetCallingAssemblyDirctory();

        public static readonly string DBPath
            = AssemblyUtilDirPath
            .PathCombine("database")
            .GetFullPath();

        public static readonly string XmlLogDirectory
            = AssemblyUtilDirPath
            .PathCombine("log")
            .GetFullPath();

        public static readonly string XmlLogPath
            = XmlLogDirectory
            .PathCombine(string.Format("{0:yyyyMM}{1}log{0:yyyyMMdd-HHmmss-FFFFFFF}.html", DateTime.UtcNow, Path.DirectorySeparatorChar))
            .GetFullPath();

        internal const string MICRO_DATA_HTML_HEADER = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
  <meta charset=""utf-8"" />
  <title>title</title>
  <link rel=""stylesheet"" href=""../../microdata.css"" />
</head>
<body>";

    }
}
