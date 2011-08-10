using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util.Text
{
    /// <summary>
    /// レーベンシュタイン距離
    /// </summary>
    public static class LevenshteinDistance
    {
        /// <summary>
        /// ２つの文字の編集距離を計算します。
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Compute(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) throw new ArgumentNullException("a");
            if (string.IsNullOrEmpty(b)) throw new ArgumentNullException("b");
            int x = a.Length;
            int y = b.Length;
            if (x == 0) return y;
            if (y == 0) return x;
            int[,] d = new int[x + 1, y + 1];
            for (int i = 0; i <= x; d[i, 0] = i++) ;
            for (int j = 0; j <= y; d[0, j] = j++) ;
            int cost = default(int);
            for (int i = 1; i <= x; i++) {
                for (int j = 1; j <= y; j++) {
                    cost = (a.Substring(i - 1, 1) != b.Substring(j - 1, 1) ? 1 : 0);
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[x, y];
        }
    }
}