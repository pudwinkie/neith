using System;
using System.Collections.Generic;
using System.Concurrency;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;

namespace System.Net
{
    public static class RxHttp
    {
        /// <summary>
        /// リクエストを非同期に実施し、レスポンスを取得します。
        /// </summary>
        /// <param name="rxReq"></param>
        /// <returns></returns>
        public static IObservable<HttpWebResponse> GetResponse(this IObservable<HttpWebRequest> rxReq)
        {
            return rxReq
                .Do(req=>{
                    Debug.WriteLine("[RxHttp::GetResponse] uri=" + req.RequestUri.AbsoluteUri);
                })
                .SelectMany(req => Observable.FromAsyncPattern<WebResponse>(
                    req.BeginGetResponse,
                    req.EndGetResponse)())
                .Select(res => res as HttpWebResponse);
        }

        #region GET
        /// <summary>
        /// GETリクエストを実行します。
        /// </summary>
        /// <param name="rxReq"></param>
        /// <returns></returns>
        public static IObservable<string> ToGetContents(this IObservable<HttpWebRequest> rxReq)
        {
            return rxReq
                .Select(req => {
                    req.Method = "GET";
                    return req;
                })
                .GetResponse()
                .ToResponseStream()
                .ToContents()
                ;
        }

        public static IObservable<Stream> ToResponseStream(this IObservable<HttpWebResponse> rxRes)
        {
            return rxRes
                .Select(res => res.GetResponseStream())
                ;
        }

        /// <summary>
        /// 指定されたURL文字列よりコンテンツを取得します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<string> RxGetWebContents(this string url)
        {
            return Observable
                .Return(WebRequest.Create(url) as HttpWebRequest, Scheduler.ThreadPool)
                .ToGetContents();
        }

        #endregion

        #region HEAD
        /// <summary>
        /// HEADリクエストを実行し、指定ヘッダを返します。
        /// </summary>
        /// <param name="rxReq"></param>
        /// <param name="headerName"></param>
        /// <returns></returns>
        public static IObservable<string> GetHeaderItem(this IObservable<HttpWebRequest> rxReq, string headerName)
        {
            return rxReq
                .Select(req => {
                    req.Method = "HEAD";
                    return req;
                })
                .GetResponse()
                .Select(res => {
                    using (res) return res.GetResponseHeader(headerName);
                });
        }

        /// <summary>
        /// HEADリクエストを実行し、指定ヘッダを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<string> RxGetResponseHeaderItem(this string url, string headerName)
        {
            return Observable
                .Return(WebRequest.Create(url) as HttpWebRequest, Scheduler.ThreadPool)
                .GetHeaderItem(headerName);
        }

        #endregion

    }
}
