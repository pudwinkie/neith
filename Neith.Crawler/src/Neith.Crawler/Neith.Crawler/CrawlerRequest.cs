using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Neith.Crawler
{
    /// <summary>
    /// クロウラーに対する要求データ。
    /// </summary>
    public class CrawlerRequest
    {
        /// <summary>このタスクが起動したときのTickCount</summary>
        private static readonly string TouchString = GetTouchString();
        private static string GetTouchString()
        {
            return DateTimeOffset.Now.Ticks.ToString();
        }

        /// <summary>Touch判定用のロック</summary>
        private static readonly object LockTouch =new object();


        /// <summary>URL</summary>
        public string URL { get; private set; }

        /// <summary>キャッシュ</summary>
        public CrawlerCache Cache { get; private set; }

        /// <summary>ETag</summary>
        public string ETag
        {
            get
            {
                if (etag != null) return etag;
                etag = Cache.ReadAllText(ETAG_KEY);
                return etag;
            }
            set
            {
                Cache.WriteAllText(ETAG_KEY, value);
                etag = value;
            }
        }
        private string etag;
        private const string ETAG_KEY="etag.txt";

        /// <summary>Date</summary>
        public DateTime Date
        {
            get
            {
                if (date != DateTime.MinValue) return date;
                var text = Cache.ReadAllText(DATE_KEY);
                if (string.IsNullOrEmpty(text)) return DateTime.MinValue;
                long a;
                if (!long.TryParse(text, out a)) return DateTime.MinValue;
                date = DateTime.FromBinary(a);
                return date;
            }
            set
            {
                var a = value.ToBinary();
                Cache.WriteAllText(DATE_KEY, a.ToString());
                date = value;
            }
        }
        private DateTime date = DateTime.MinValue;
        private const string DATE_KEY = "date.txt";


        /// <summary>このリクエストをこのタスクが既に処理済ならtrue。</summary>
        public bool IsTouch
        {
            get
            {
                if (touch == null) {
                    touch = Cache.ReadAllText(TOUCH_KEY);
                }
                return TouchString == touch;
            }
        }

        /// <summary>
        /// 処理済マークをつける。
        /// </summary>
        public void Touch()
        {
            Cache.WriteAllText(TOUCH_KEY, TouchString);
            touch = TouchString;
        }
        private string touch;
        private const string TOUCH_KEY = "touch.txt";

        /// <summary>
        /// タッチ判定を行い、タッチされていなければタッチする。
        /// タッチされてたかどうかを返す。
        /// この処理はシステム内で同期されます。
        /// </summary>
        /// <returns></returns>
        public bool CheckTouch()
        {
            lock (LockTouch) {
                var rc = IsTouch;
                if (!rc) Touch();
                return rc;
            }
        }

        /// <summary>WebRequest</summary>
        public HttpWebRequest Request { get; set; }


        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="url"></param>
        public CrawlerRequest(string url)
        {
            URL = (new Uri(url)).AbsoluteUri;
            Cache = CrawlerCache.Create(URL);
        }

    }
}
