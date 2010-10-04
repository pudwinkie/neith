using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Neith.Crawler
{
    /// <summary>
    /// クロウラーの応答データ。
    /// </summary>
    public class CrawlerResponse : IDisposable
    {
        /// <summary>リクエスト情報</summary>
        public CrawlerRequest Request { get; private set; }

        /// <summary>レスポンス</summary>
        public HttpWebResponse Response { get; private set; }

        /// <summary>キャッシュ</summary>
        public CrawlerCache Cache { get { return Request.Cache; } }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="cReq"></param>
        /// <param name="res"></param>
        public CrawlerResponse(CrawlerRequest cReq, WebResponse res)
        {
            Request = cReq;
            Response = res as HttpWebResponse;
        }

        /// <summary>
        /// リソースの開放を行ないます。
        /// </summary>
        public void Dispose()
        {
            if (Response == null) return;
            Response.Close();
            Response = null;
        }

    }
}
