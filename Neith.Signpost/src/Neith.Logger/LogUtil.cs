using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Neith.Logger
{
    public static class LogUtil
    {
        /// <summary>
        /// 日付よりログのPathを作成し、文字列を返します。
        /// </summary>
        /// <param name="date"></param>
        /// <param name="isDirCreate">ディレクトリを作成するならtrue</param>
        /// <returns></returns>
        public static string GetPath(DateTime date, bool isDirCreate)
        {
            var dir = Path.Combine(
                Const.Folders.Log,
                string.Format("{0:yyyy}", date),
                string.Format("{0:MM}", date));
            var path = Path.Combine(dir, string.Format("{0:yyyy-MM-dd}.log", date));
            if (isDirCreate) Directory.CreateDirectory(dir);
            return path;
        }

    }
}
