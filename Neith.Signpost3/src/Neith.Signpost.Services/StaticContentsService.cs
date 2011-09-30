using System;
using System.IO;
using System.ServiceModel.Web;
using Neith.Util.Reflection;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// 静的コンテンツへのアクセスサービス。
    /// </summary>
    public class StaticContentsService : IStaticContents
    {
        #region 静的ファイルの操作

        private static readonly string BaseDir =
            AssemblyUtil.GetCallingAssemblyDirctory()
            .PathCombine("Contents");

        private static readonly Lazy<byte[]> CacheHTML
            = new Lazy<byte[]>(() => File.ReadAllBytes(BaseDir.PathCombine("Neith.Signpost.SLTestPage.html")));

        private static readonly Lazy<byte[]> CacheJS
            = new Lazy<byte[]>(() => File.ReadAllBytes(BaseDir.PathCombine("Silverlight.js")));

        private static readonly Lazy<byte[]> CacheXAP
            = new Lazy<byte[]>(() => File.ReadAllBytes(BaseDir.PathCombine("Neith.Signpost.SL.xap")));


        public Stream GetHtml(string a)
        {
            var st = new MemoryStream(CacheHTML.Value);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html; charset=utf-8";
            return st;
        }

        public Stream GetJS(string a)
        {
            if (a != "Silverlight") throw new ArgumentException("not Silverlight.js");
            var st = new MemoryStream(CacheJS.Value);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/javascript; charset=utf-8";
            return st;
        }

        public Stream GetXap(string a)
        {
            if (a != "Neith.Signpost.SL") throw new ArgumentException("not Neith.Signpost.SL.xap");
            var st = new MemoryStream(CacheXAP.Value);
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-silverlight-app";
            return st;
        }

        #endregion
    }
}
