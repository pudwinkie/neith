using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util.Text
{
    /// <summary>
    /// N-gram。
    /// 検索する文字列が完全に含まれるときに１、まったく含まれないときに０を返す。
    /// </summary>
    public class Ngram
    {
        /// <summary>
        /// Unigram
        /// </summary>
        /// <param name="find">検索する文字列</param>
        /// <param name="target">検索対象文字列</param>
        /// <returns></returns>
        public static double CompareUnigram(string find, string target)
        {
            return Compare(1, find, target);
        }

        /// <summary>
        /// Bigram
        /// </summary>
        /// <param name="find">検索する文字列</param>
        /// <param name="target">検索対象文字列</param>
        /// <returns>
        /// </returns>
        public static double CompareBigram(string find, string target)
        {
            return Compare(2, find, target);
        }

        /// <summary>
        /// Trigram
        /// </summary>
        /// <param name="find">検索する文字列</param>
        /// <param name="target">検索対象文字列</param>
        /// <returns></returns>
        public static double CompareTrigram(string find, string target)
        {
            return Compare(3, find, target);
        }

        private static double Compare(int n, string find, string target)
        {
            if (find == null) throw new ArgumentNullException("scan");
            if (target == null) throw new ArgumentNullException("text");

            List<string> noise = new List<string>();
            for (int i = 0; i < find.Length - (n - 1); i++) {
                string ngitem = find.Substring(i, n);
                if (!noise.Contains(ngitem)) { noise.Add(ngitem); }
            }
            if (noise.Count == 0) return 0;

            int coincidence = 0;
            for (int i = 0; i < target.Length - (n - 1); i++) {
                string ngitem = target.Substring(i, n);
                if (noise.Contains(ngitem)) { coincidence++; }
            }
            return (double)coincidence / noise.Count;
        }
    }
}