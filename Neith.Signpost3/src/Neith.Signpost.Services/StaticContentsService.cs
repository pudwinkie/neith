using System;
using System.IO;
using System.ServiceModel.Web;
using Neith.Util.Reflection;
using System.Windows;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// 静的コンテンツへのアクセスサービス。
    /// </summary>
    public class StaticContentsService : IStaticContents
    {
        #region 静的ファイルの操作

        private static Stream GetStream(string name)
        {
            var uri = new Uri("/Contents/" + name,  UriKind.Relative);
            var info = Application.GetResourceStream(uri);
            return info.Stream;
        }


        private static readonly Lazy<Stream> CacheHTML
            = new Lazy<Stream>(() => GetStream("Neith.Signpost.SLTestPage.html"));

        private static readonly Lazy<Stream> CacheJS
            = new Lazy<Stream>(() => GetStream("Silverlight.js"));

        private static readonly Lazy<Stream> CacheXAP
            = new Lazy<Stream>(() => GetStream("Neith.Signpost.SL.xap"));


        public Stream GetHtml(string a)
        {
            var st = CacheHTML.Value;
            st.Seek(0, SeekOrigin.Begin);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html; charset=utf-8";
            return st;
        }

        public Stream GetJS(string a)
        {
            if (a != "Silverlight") throw new ArgumentException("not Silverlight.js");
            var st = CacheJS.Value;
            st.Seek(0, SeekOrigin.Begin);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/javascript; charset=utf-8";
            return st;
        }

        public Stream GetXap(string a)
        {
            if (a != "Neith.Signpost.SL") throw new ArgumentException("not Neith.Signpost.SL.xap");
            var st = CacheXAP.Value;
            st.Seek(0, SeekOrigin.Begin);
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-silverlight-app";
            return st;
        }

        #endregion
    }
}
