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
