using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Logger.Model;
using System.Runtime.Serialization;
using System.IO;
using ProtoBuf;

namespace Neith.Logger
{
    public class LogStore : IDisposable
    {
        private static LogStore instance;
        static LogStore()
        {
            StoreInit();
        }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public static void StoreInit()
        {
            StoreClose();
            instance = new LogStore();
        }

        /// <summary>
        /// インスタンスを取得します。
        /// </summary>
        public static LogStore Instance { get { return instance; } }

        /// <summary>
        /// インスタンスを開放します。
        /// </summary>
        public static void StoreClose()
        {
            if (instance == null) return;
            instance.Dispose();
            instance = null;
        }

        private Stream stream = null;
        private DateTime lastDate = DateTime.MinValue.Date;

        private LogStore()
        {
        }

        /// <summary>
        /// 解放処理。
        /// </summary>
        public void Dispose()
        {
            SteramClose();
        }

        /// <summary>
        /// ストリームをクローズする。
        /// </summary>
        public void SteramClose()
        {
            if (stream == null) return;
            stream.Flush();
            stream.Dispose();
            stream = null;
        }

        /// <summary>
        /// ログの日付を確認し、状況に応じて書込み先を切り替える。
        /// </summary>
        /// <param name="dateTime"></param>
        private void DateCheck(DateTime dateTime)
        {
            // チェック
            var date = dateTime.Date;
            if (lastDate != date) SteramClose();
            if (stream != null) return;
            // ファイルの作成
            lastDate = date;
            var dir = Path.Combine(
                Const.Folders.Log,
                string.Format("{0:yyyy}", lastDate),
                string.Format("{0:MM}", lastDate));
            var path = Path.Combine(dir, string.Format("{0:yyyy-MM-dd}.log", lastDate));
            Directory.CreateDirectory(dir);
            stream = File.OpenWrite(path);
        }

        /// <summary>
        /// ログを書き込みます。
        /// </summary>
        /// <param name="log"></param>
        public void Store(Log log)
        {
            DateCheck(log.TimestampUTC);
            Serializer.Serialize(stream, log);
        }

    }
}
