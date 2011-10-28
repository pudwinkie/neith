using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Neith.Util.Reflection;

namespace Neith.Signpost.Logger.Test
{
    public static class Const
    {
        public static readonly string AssemblyUtilDirPath = AssemblyUtil.GetCallingAssemblyDirctory();

        public static readonly string TestDataRootPath
            = AssemblyUtilDirPath
            .PathCombine("..", "..", "..", "..", "test")
            .GetFullPath();

        public static readonly string DBPath
            = TestDataRootPath
            .PathCombine("database")
            .GetFullPath();

        public static readonly string XmlLogPath
            = TestDataRootPath
            .PathCombine("log", "log.html")
            .GetFullPath();


    }
}
