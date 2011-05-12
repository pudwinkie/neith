using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Neith.Logger.Model;

namespace Neith.Logger
{
    public class LogLoader : IDisposable
    {
        private Stream stream = null;
        private DateTime lastDate = DateTime.MinValue;

        public void Dispose()
        {
            SteramClose();
        }

        public void SteramClose()
        {
            if (stream == null) return;
            stream.Dispose();
            stream = null;
            lastDate = DateTime.MinValue;
        }

        /// <summary>
        /// 日付を確認し、状況に応じて読み込み先を切り替える。
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
            var path = LogUtil.GetPath(date, false);
            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        /// <summary>
        /// ログを読み込みます。
        /// </summary>
        /// <param name="pos">ログの場所情報</param>
        public NeithLog Load(NeithLogStorePosition pos)
        {
            DateCheck(pos.Date);
            stream.Seek(pos.Positon, SeekOrigin.Begin);
            var rc = stream.Deserialize<NeithLog>();
            return rc;
        }

    }
}
