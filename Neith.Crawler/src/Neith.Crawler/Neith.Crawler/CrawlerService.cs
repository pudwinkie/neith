using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Neith.Crawler
{
    /// <summary>
    /// Webクローラサービス
    /// </summary>
    public static class CrawlerService
    {
        public static IObservable<WebResponse> RxGetResponse(this WebRequest req)
        {
            return Observable.FromAsyncPattern<WebResponse>(
                req.BeginGetResponse, req.EndGetResponse)();
        }

        public static IObservable<HttpWebResponse> GetResponse(this IObservable<HttpWebRequest> rxReq)
        {
            return rxReq
                .SelectMany(req => req.RxGetResponse())
                .Select(res => res as HttpWebResponse);
        }

        public static IObservable<string> GetWebContents(this IObservable<HttpWebRequest> rxReq)
        {
            return rxReq
                .Select(req => {
                    req.Method = "GET";
                    return req;
                })
                .GetResponse()
                .Select(res => {
                    using (var st = res.GetResponseStream())
                    using (var reader = new StreamReader(st, Encoding.UTF8)) {
                        return reader.ReadToEnd();
                    }
                });
        }

        public static IObservable<string> GetResponseHeader(this IObservable<HttpWebRequest> rxReq, string headerName)
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
        /// 指定されたURL文字列よりコンテンツを取得します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<string> RxGetWebContents(this string url)
        {
            return Observable
                .Start(() => { return WebRequest.Create(url) as HttpWebRequest; })
                .GetWebContents();
        }


        /// <summary>
        /// 指定されたURLのコンテンツよりレスポンスヘッダを取得します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<string> RxGetResponseHeader(this string url, string headerName)
        {
            return Observable
                .Start(() => { return WebRequest.Create(url) as HttpWebRequest; })
                .GetResponseHeader(headerName);
        }

    }
}