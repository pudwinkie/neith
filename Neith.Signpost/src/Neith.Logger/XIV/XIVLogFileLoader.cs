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
    /// このクラスは開放を行うまで処理を終了しません。
    /// </summary>
    internal class XIVLogFileLoader : IDisposable
    {
        private IEnumerator<NeithLog> reader;

        public XIVLogFileLoader(string path)
        {
            reader = EnReadTask(path).GetEnumerator();
        }

        public void Dispose()
        {
            if (reader == null) return;
            reader.Dispose();
            reader = null;
        }

        /// <summary>
        /// 新しいログの存在を確認し、読み込めるだけ返します。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<NeithLog> Polling()
        {
            while (reader.MoveNext()) {
                if (reader.Current == null) yield break;
                yield return reader.Current;
            }
        }

        private class TaskData
        {
            public FileStream Stream;
            public long LastLength = long.MinValue;
            public byte[] Buf = new byte[1024];
            public int Offset = 0;
            public int Remain = 0;
        }

        /// <summary>
        /// 読み込みタスク本体。中断する必要がある場合はnullを返します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<NeithLog> EnReadTask(string path)
        {
            path = Path.GetFullPath(path);
            using (var st = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                st.Seek(0, SeekOrigin.Begin);
                var t = new TaskData();
                t.Stream = st;

                // ヘッダ読み込み処理
                while (ReadWait(t)) yield return null;



                // メインループ


                while (ChangeWait(t)) yield return null;

            }


        }


        /// <summary>
        /// ファイルサイズが変動するまでtrueを返します。
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool ChangeWait(TaskData t)
        {
            var len = t.Stream.Length;
            if (t.LastLength == len) return true;
            t.LastLength = len;
            return false;
        }

        /// <summary>
        /// 指定サイズだけバッファに読み込みます。読みきれなかった場合はtrueを返します。
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private bool ReadWait(TaskData t)
        {
            var reqCount = t.LastLength - t.Stream.Position;
            if (reqCount > t.Remain) reqCount = t.Remain;

            var cnt = t.Stream.Read(t.Buf, t.Offset, (int)reqCount);
            t.Offset += cnt;
            t.Remain -= cnt;
            if (t.Remain == 0) return false;
            ChangeWait(t);
            return true;
        }



    }
}
