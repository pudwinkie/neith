using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;

namespace Neith.Crawler
{
    /// <summary>
    /// Webクローラサービス
    /// </summary>
    public static class CrawlerService
    {
        #region URLから直接取得する非同期処理


        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツの更新にかかわらず内容を取得します。
        /// 取得できなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<CrawlerResponse> RxGetCrowlAny(this string url)
        {
            return url
                .RxStartAny()
                .RxCrowlImpl()
                ;
        }

        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツに更新があった場合にレスポンスを返します。
        /// 更新がない場合、内容がなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<CrawlerResponse> RxGetCrowlUpdate(this string url)
        {
            return url
                .RxStartUpdate()
                .RxCrowlImpl()
                ;
        }

        /// <summary>
        /// 更新コンテンツの取得リクエスト開始。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IObservable<CrawlerRequest> RxStartUpdate(this string url)
        {
            return Observable
                .Return(url)
                .ToCrawlerRequest()
                ;
        }

        /// <summary>
        /// 無条件取得コンテンツの取得リクエスト開始。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IObservable<CrawlerRequest> RxStartAny(this string url)
        {
            return url
                .RxStartUpdate()
                .Do(req => { req.Cache.Clear(); })
                ;
        }

        #endregion

        #region リクエスト

        /// <summary>
        /// クローラリクエストに変換。
        /// </summary>
        /// <param name="rxUrl"></param>
        /// <returns></returns>
        public static IObservable<CrawlerRequest> ToCrawlerRequest(this IObservable<string> rxUrl)
        {
            return rxUrl
                .Select(url => { return new CrawlerRequest(url); })
                ;
        }

        #endregion


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
                    if (etag != null) {
                        if (etag == cRes.Request.ETag) return null;
                        cRes.Request.ETag = etag;
                    }
                    return cRes;
                })
                ;
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
                        async => {
                            var res = cReq.Request.EndGetResponse(async);
                            cRes = new CrawlerResponse(cReq, res);
                            return cRes;
                        })())
                .Finally(() => { cRes.Dispose(); });
        }



        #region レスポンス変換
        public static IObservable<Stream> ToResponseStream(this IObservable<CrawlerResponse> rxRes)
        {
            return rxRes
                .Select(cRes => {
                if (cRes == null) return null;
                return cRes.Response.GetResponseStream();
            });
        }

        public static IObservable<string> ToContents(this IObservable<CrawlerResponse> rxRes)
        {
            return rxRes
                .ToResponseStream()
                .ToContents()
                ;
        }


        #endregion

    }
}