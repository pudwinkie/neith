using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Crawler
{
    /// <summary>
    /// クロウラーの応答データ。
    /// </summary>
    public class CrawlerResponse
    {
        /// <summary>リクエスト情報</summary>
        public CrawlerRequest Request { get; private set; }

        /// <summary>キャッシュ</summary>
        public CrawlerCache Cache { get { return Request.Cache; } }

    }
}
