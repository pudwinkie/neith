using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Neith.Logger.Model;

namespace Neith.Logger.XIV
{
    /// <summary>
    /// ログファイル１本を監視し、Logを抽出します。
    /// </summary>
    internal class XIVLogFileLoader : IDisposable
    {
        private FileStream stream;
        private long lastLength = long.MinValue;

        public XIVLogFileLoader(string path)
        {
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            stream.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            if (stream == null) return;
            stream.Dispose();
            stream = null;
        }

        /// <summary>
        /// 新しいログの存在を確認し、読み込めるだけ返します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Log> Polling()
        {
            if (stream.Length == lastLength) yield break;
            lastLength = stream.Length;


        }

    }
}
