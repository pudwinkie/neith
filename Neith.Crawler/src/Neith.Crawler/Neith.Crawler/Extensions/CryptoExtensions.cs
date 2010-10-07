using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace System.Security.Cryptography
{
    /// <summary>
    /// ユーティリィ拡張
    /// </summary>
    public static class CryptoExtensions
    {
        /// <summary>
        /// 文字列をハッシュ変換します。
        /// 現在の実装はUTF8→SHA1→BASE64(URLsafe)です。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToHashString(this string text)
        {
            return text
                .ToBytesUtf8()
                .ToSHA1()
                .ToBase64Urlsafe();
        }

        public static byte[] ToBytesUtf8(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        public static byte[] ToSHA1(this byte[] data)
        {
            if(hashEngine==null) hashEngine = HashAlgorithm.Create("SHA1");
            return hashEngine.ComputeHash(data);
        }
        [ThreadStatic]
        private static HashAlgorithm hashEngine;

        public static string ToBase64Urlsafe(this byte[] data)
        {
            var str = Convert.ToBase64String(data);
            return str.Replace('+', '-').Replace('/', '_').Replace('=',' ').TrimEnd();
        }

    }
}
