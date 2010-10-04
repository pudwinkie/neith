using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
#if WINDOWS_PHONE
using Microsoft.Phone.Reactive;
#endif

namespace Codeplex.OAuth
{
    internal static class Utility
    {
        static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTime(this DateTime target)
        {
            return (long)(target - unixEpoch).TotalSeconds;
        }

        public static string UrlEncode(this string stringToEscape)
        {
            return Uri.EscapeDataString(stringToEscape);
        }

        public static string Wrap(this string input, string wrapper)
        {
            return wrapper + input + wrapper;
        }

        public static string ToString<T>(this IEnumerable<T> source, string separator)
        {
            var index = 0;
            return source.Aggregate(new StringBuilder(),
                    (sb, o) => (index++ == 0) ? sb.Append(o) : sb.AppendFormat("{0}{1}", separator, o))
                .ToString();
        }

        public static string ReadToEnd(this Stream stream)
        {
            using (stream)
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        public static IObservable<WebResponse> GetResponseAsObservable(this WebRequest webRequest)
        {
            return Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse)();
        }

        public static IObservable<Stream> GetRequestStreamAsObservable(this WebRequest webRequest)
        {
            return Observable.FromAsyncPattern<Stream>(webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream)();
        }

        public static IObservable<Unit> WriteAsObservable(this Stream stream, byte[] buffer, int offset, int count)
        {
            // Windows Phone 7 doesn't have <T1,T2,T3,Unit> overload
            return Observable.FromAsyncPattern((ac, o) => stream.BeginWrite(buffer, offset, count, ac, o), stream.EndWrite)();
        }
    }
}