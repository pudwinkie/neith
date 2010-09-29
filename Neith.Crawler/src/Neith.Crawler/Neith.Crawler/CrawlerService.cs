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

        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツに更新があった場合にレスポンスストリームを返します。
        /// 更新がない場合、内容がなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<Stream> RxGetUpdateWebResponseStream(this string url)
        {
            return url
                .RxStartUpdate()
                .RxCrowlImpl()
                .GetResponseStream();
        }


        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツに更新があった場合に内容を返します。
        /// 更新がない場合、内容がなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<string> RxGetUpdateWebContents(this string url)
        {
            return url
                .RxStartUpdate()
                .RxCrowlImpl()
                .GetContents();
        }

        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツの更新にかかわらず内容を取得します。
        /// 取得できなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<string> RxGetExWebContents(this string url)
        {
            return url
                .RxStartEx()
                .RxCrowlImpl()
                .GetContents();
        }



        /// <summary>
        /// 更新コンテンツの取得リクエスト開始。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IObservable<CrawlerRequest> RxStartUpdate(this string url)
        {
            return Observable
                .Start(() => { return new CrawlerRequest(url); });
        }

        /// <summary>
        /// 無条件取得コンテンツの取得リクエスト開始。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IObservable<CrawlerRequest> RxStartEx(this string url)
        {
            return Observable
                .Start(() => {
                    var req = new CrawlerRequest(url);
                    req.Cache.Clear();
                    return req;
                });
        }

        /// <summary>
        /// クロール処理本体。
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private static IObservable<CrawlerResponse> RxCrowlImpl(this IObservable<CrawlerRequest> rxReq)
        {
            return rxReq
                // リクエストの作成
                .Select(cReq => {
                    var req = WebRequest.Create(cReq.URL) as HttpWebRequest;
                    cReq.Request = req;
                    req.Method = "GET";
                    var etag = cReq.ETag;
                    if (!string.IsNullOrEmpty(etag)) {
                        req.Headers[HttpRequestHeader.IfNoneMatch] = etag;
                    }
                    return cReq;
                })
                // レスポンスの処理
                .GetResponse()
                .Select(cRes => {
                    var res = cRes.Response;
                    switch (res.StatusCode) {
                        case HttpStatusCode.PreconditionFailed: return null;
                        case HttpStatusCode.OK: break;
                        default:
                            throw new IOException(string.Format(
                                "不正なWebレスポンスを受理：code={0}\n{1}",
                                res.StatusCode, res.StatusDescription));
                    }
                    var etag = res.Headers[HttpResponseHeader.ETag];
                    if (etag == cRes.Request.ETag) return null;
                    cRes.Request.ETag = etag;
                    return cRes;
                });
        }

        private static IObservable<Stream> GetResponseStream(this IObservable<CrawlerResponse> rxRes)
        {
            return rxRes.Select(cRes => {
                if (cRes == null) return null;
                return cRes.Response.GetResponseStream();
            });
        }

        private static IObservable<string> GetContents(this IObservable<CrawlerResponse> rxRes)
        {
            return rxRes.Select(cRes => {
                if (cRes == null) return null;
                var res = cRes.Response;
                using (var st = res.GetResponseStream())
                using (var reader = new StreamReader(
                    st, Encoding.GetEncoding(res.CharacterSet))) {
                    return reader.ReadToEnd();
                }
            });
        }

        private static IObservable<CrawlerResponse> GetResponse(this IObservable<CrawlerRequest> rxReq)
        {
            CrawlerResponse cRes = null;
            return rxReq
                .Select(cReq => {
                    var req = cReq.Request;

                    return cReq;
                })
                .SelectMany(cReq => Observable
                    .FromAsyncPattern<CrawlerResponse>(
                        cReq.Request.BeginGetResponse,
                        (async) => {
                            var res = cReq.Request.EndGetResponse(async);
                            cRes = new CrawlerResponse(cReq, res);
                            return cRes;
                        })())
                .Finally(() => { cRes.Dispose(); });
        }


    }
}