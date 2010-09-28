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
                .ToUtf8Bytes()
                .ToSHA1()
                .ToBase64Urlsafe();
        }

        public static byte[] ToUtf8Bytes(this string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }
        public static byte[] ToSHA1(this byte[] data)
        {
            return hashEngine.ComputeHash(data);
        }
        private static readonly HashAlgorithm hashEngine = HashAlgorithm.Create("SHA1");
        public static string ToBase64Urlsafe(this byte[] data)
        {
            var str = Convert.ToBase64String(data);
            return str.Replace('+', '-').Replace('/', '_').Replace('=',' ').TrimEnd();
        }

    }
}
