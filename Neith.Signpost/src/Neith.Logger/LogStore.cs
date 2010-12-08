using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Neith.Logger.Model;
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
        public static LogStore Instance
        {
            get
            {
                if (instance == null) StoreInit();
                return instance;
            }
        }

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
            var path = LogUtil.GetPath(date, true);
            stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            stream.Seek(0, SeekOrigin.End);
        }

        /// <summary>
        /// ログを書き込みます。
        /// </summary>
        /// <param name="log"></param>
        public LogStorePosition Store(Log log)
        {
            DateCheck(log.TimestampUTC);
            var rc = new LogStorePosition(lastDate, stream.Position);
            Serializer.Serialize(stream, log);
            return rc;
        }

        public void Flush()
        {
            if (stream == null) return;
            stream.Flush();
        }
    }
}
