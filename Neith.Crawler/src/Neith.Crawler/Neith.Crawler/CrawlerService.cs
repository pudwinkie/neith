using System;
using System.Collections.Generic;
using System.Concurrency;
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
        /// <param name="rxUrl"></param>
        /// <returns></returns>
        public static IObservable<CrawlerResponse> TpCrowlAny(this IObservable<string> rxUrl)
        {
            return rxUrl
                .ToCrawlerRequest()
                .Do(req => { req.Cache.Clear(); })
                .RxCrowlImpl()
                ;
        }

        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツの更新にかかわらず内容を取得します。
        /// 取得できなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<CrawlerResponse> RxGetCrowlAny(this string url)
        {
            return Observable
                .Return(url, Scheduler.ThreadPool)
                .TpCrowlAny()
                ;
        }



        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツに更新があった場合にレスポンスを返します。
        /// 更新がない場合、内容がなかった場合はnullを返します。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<CrawlerResponse> ToCrowlUpdate(this IObservable<string> rxUrl)
        {
            return rxUrl
                .ToCrawlerRequest()
                .RxCrowlImpl()
                ;
        }
        /// <summary>
        /// 指定されたURLのクロールを発行します。
        /// コンテンツに更新があった場合にレスポンスを返します。
        /// 更新がない場合、内容がなかった場合はnullを返します。
        /// 戻り値のCrawlerResponseは処理後に開放してください。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static IObservable<CrawlerResponse> RxGetCrowlUpdate(this string url)
        {
            return Observable
                .Return(url, Scheduler.ThreadPool)
                .ToCrowlUpdate()
                ;
        }


        /// <summary>
        /// ページを読み込むクローラを定義します。
        /// </summary>
        /// <returns></returns>
        public static IObservable<Unit> RxPageCrowl(this string startUrl
            , Func<XElement, string> getNextUrl
            , Func<IObservable<XElement>, IObservable<Unit>> rxParse)
        {
            var pipe = startUrl
                .EnPageCrowl(getNextUrl)
                .ToObservable(Scheduler.TaskPool)
                ;
            return rxParse(pipe);
        }

        /// <summary>
        /// ページ要素を列挙するクローラを定義します。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="getNextUrl"></param>
        /// <returns></returns>
        public static IEnumerable<XElement> EnPageCrowl(this string url
            , Func<XElement, string> getNextUrl)
        {
            return url
                .EnPageCrowlImpl(getNextUrl)
                .AsParallel()
                ;
        }
        private static IEnumerable<XElement> EnPageCrowlImpl(this string url
            , Func<XElement, string> getNextUrl)
        {
            while (!string.IsNullOrEmpty(url)) {
                var pipe = Observable.Return(url, Scheduler.ThreadPool)
                    .ToCrowlUpdate()
                    .ToResponseStream()
                    .ToXHtmlElement()
                    ;
                var doc = pipe.FirstOrDefault();
                if (doc == null) yield break;
                yield return doc;
                url = getNextUrl(doc);
            }
        }


                /// <summary>
        /// ページを読み込むクローラを定義します。
        /// </summary>
        /// <returns></returns>
        public static IObservable<Unit> RxPageCrowl(this string startUrl
            , Func<XElement, string> getNextUrl
            , Func<XElement, Unit> parse)
        {
            return startUrl
                .RxPageCrowl(getNextUrl, rxDoc => {
                    return rxDoc
                        .Select(parse)
                        ;
                });
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
                // 既に処理済なら止める
                .Where(cReq => !cReq.CheckTouch())
                // リクエストの作成
                .Do(cReq => {
                    var req = WebRequest.Create(cReq.URL) as HttpWebRequest;
                    cReq.Request = req;
                    req.Method = "GET";
                    var etag = cReq.ETag;
                    if (!string.IsNullOrEmpty(etag)) {
                        req.Headers[HttpRequestHeader.IfNoneMatch] = etag;
                    }
                    var date = cReq.Date;
                    if (date > DateTime.MinValue) {
                        req.IfModifiedSince = date;
                    }

                })
                // レスポンスの処理、処理済なら止める
                .GetResponse()
                .Where(cRes => {
                    var res = cRes.Response;
                    switch (res.StatusCode) {
                        case HttpStatusCode.PreconditionFailed: return false;
                        case HttpStatusCode.OK: break;
                        default:
                            throw new IOException(string.Format(
                                "不正なWebレスポンスを受理：code={0}\n{1}",
                                res.StatusCode, res.StatusDescription));
                    }
                    var etag = res.Headers[HttpResponseHeader.ETag];
                    if (etag != null) {
                        if (etag == cRes.Request.ETag) return false;
                        cRes.Request.ETag = etag;
                    }
                    var date = res.LastModified;
                    if (date <= cRes.Request.Date) return false;
                    if (date != DateTime.MaxValue) cRes.Request.Date = date;

                    return true;
                })
                ;
        }

        private static DateTime ToDateExact(this string text)
        {
            var date = DateTime.MaxValue;
             System.DateTime.TryParseExact(text,
                         expectedFormats,
                         System.Globalization.DateTimeFormatInfo.InvariantInfo,
                         System.Globalization.DateTimeStyles.None,
                         out date);
             return date;
        }
        private static readonly string[] expectedFormats = { "ddd, d MMM yyyy HH':'mm':'ss zzz", "r" };


        /// <summary>
        /// CrawlerResponseを取得する。
        /// </summary>
        /// <param name="rxReq"></param>
        /// <returns></returns>
        private static IObservable<CrawlerResponse> GetResponse(this IObservable<CrawlerRequest> rxReq)
        {
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
                            return new CrawlerResponse(cReq, res);
                        })())
                ;
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