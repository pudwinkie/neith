using System;
using System.IO;
using System.ServiceModel.Web;
using Neith.Util.Reflection;
using System.Windows;
using System.Windows.Resources;

namespace Neith.Signpost.Services
{
    /// <summary>
    /// 静的コンテンツへのアクセスサービス。
    /// </summary>
    public class StaticContentsService : IStaticContents
    {
        #region 静的ファイルの操作

        private static StreamResourceInfo GetStreamInfo(string name)
        {
            var uri = new Uri("/Contents/" + name,  UriKind.Relative);
            return Application.GetResourceStream(uri);
        }


        public Stream GetHtml(string a)
        {
            var st = GetStreamInfo("Neith.Signpost.SLTestPage.html").Stream;
            st.Seek(0, SeekOrigin.Begin);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html; charset=utf-8";
            return st;
        }

        public Stream GetJS(string a)
        {
            if (a != "Silverlight") throw new ArgumentException("not Silverlight.js");
            var st = GetStreamInfo("Silverlight.js").Stream;
            st.Seek(0, SeekOrigin.Begin);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/javascript; charset=utf-8";
            return st;
        }

        public Stream GetXap(string a)
        {
            if (a != "Neith.Signpost.SL") throw new ArgumentException("not Neith.Signpost.SL.xap");
            var st = GetStreamInfo("Neith.Signpost.SL.xap").Stream;
            st.Seek(0, SeekOrigin.Begin);
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-silverlight-app";
            return st;
        }

        #endregion
    }
}
