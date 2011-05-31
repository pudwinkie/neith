using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Neith.Util.Reflection;
using Neith.Util.IO;

namespace Neith.Logger
{
    /// <summary>定数定義</summary>
    public static class Const
    {
        /// <summary>フォルダ</summary>
        public static class Folders
        {
            /// <summary>アプリケーションフォルダ</summary>
            public static string Application { get { return AssemblyUtil.GetCallingAssemblyDirctory(); } }

            /// <summary>ログフォルダ</summary>
            public static string Log
            {
                get
                {
                    var path = Properties.Settings.Default.LogFolder;
                    if (string.IsNullOrEmpty(path)) {
                        path = Path.Combine(Application, "data", "log");
                    }
                    path = Path.GetFullPath(path);
                    return path;
                }
            }

            /// <summary>Indexフォルダ</summary>
            public static string Index
            {
                get
                {
                    var path = Properties.Settings.Default.LogFolder;
                    if (string.IsNullOrEmpty(path)) {
                        path = Path.Combine(Application, "data", "log");
                    }
                    path = Path.GetFullPath(path);
                    return path;
                }
            }


        }

    }
}
