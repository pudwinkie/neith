using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Neith.Util.Text
{
    /// <summary>
    /// Diff操作のオプション指定。
    /// </summary>
    [Serializable]
    public struct DiffOption
    {
        /// <summary>２つ以上の空白文字を集約するならtrue。</summary>
        public bool TrimSpace;

        /// <summary>空白を無視するならtrue。</summary>
        public bool IgnoreSpace;

        /// <summary>大文字小文字を無視するならtrue。</summary>
        public bool IgnoreCase;
    }

    /// <summary>
    /// 差分結果。
    /// </summary>
    [Serializable]
    public struct DiffResult
    {
        /// <summary>修正があるならtrue</summary>
        public readonly bool Modified;

        /// <summary>元文章の差分開始位置</summary>
        public readonly int OriginalStart;

        /// <summary>元文章の差分長</summary>
        public readonly int OriginalLength;

        /// <summary>比較文章の差分開始位置</summary>
        public readonly int ModifiedStart;

        /// <summary>比較文章の差分長</summary>
        public readonly int ModifiedLength;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="modified"></param>
        /// <param name="orgStart"></param>
        /// <param name="orgLength"></param>
        /// <param name="modStart"></param>
        /// <param name="modLength"></param>
        public DiffResult(bool modified, int orgStart, int orgLength, int modStart, int modLength)
        {
            this.Modified = modified;
            this.OriginalStart = orgStart;
            this.OriginalLength = orgLength;
            this.ModifiedStart = modStart;
            this.ModifiedLength = modLength;
        }

        /// <summary>
        /// テキスト表現を返します。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ((this.Modified) ? "Modified" : "Common")
                + ", Os:" + this.OriginalStart.ToString() + ", Ol:" + this.OriginalLength.ToString()
                + ", Ms:" + this.ModifiedStart.ToString() + ", Ml:" + this.ModifiedLength.ToString();
        }
    }

    /// <summary>
    /// 高速差分モジュール。
    /// </summary>
    public class FastDiff
    {
        private int[] dataA, dataB;
        private bool isSwap;
        private Snake[] fp;

        private FastDiff() { }

        /// <summary>複数行の文字列を行単位で比較します</summary>
        /// <param name="textA">元テキスト</param>
        /// <param name="textB">変更テキスト</param>
        /// <returns>比較結果</returns>
        public static DiffResult[] Diff(string textA, string textB)
        {
            DiffOption option = new DiffOption();
            return Diff(textA, textB, option);
        }

        /// <summary>複数行の文字列を行単位で比較します</summary>
        /// <param name="textA">元テキスト</param>
        /// <param name="textB">変更テキスト</param>
        /// <param name="option">オプション指定</param>
        /// <returns>比較結果</returns>
        public static DiffResult[] Diff(string textA, string textB, DiffOption option)
        {
            if (string.IsNullOrEmpty(textA) || string.IsNullOrEmpty(textB))
                return StringNullOrEmpty(textA, textB);

            FastDiff diff = new FastDiff();
            if (textA.Length <= textB.Length) {
                diff.SplitHash(textA, textB, option);
            }
            else {
                diff.isSwap = true;
                diff.SplitHash(textB, textA, option);
            }
            return diff.DetectDiff();
        }

        /// <summary>単一行の各文字を比較します</summary>
        /// <param name="textA">元テキスト</param>
        /// <param name="textB">変更テキスト</param>
        /// <returns>比較結果</returns>
        public static DiffResult[] DiffChar(string textA, string textB)
        {
            DiffOption option = new DiffOption();
            return DiffChar(textA, textB, option);
        }

        /// <summary>単一行の各文字を比較します</summary>
        /// <param name="textA">元テキスト</param>
        /// <param name="textB">変更テキスト</param>
        /// <param name="option">オプション指定</param>
        /// <returns>比較結果</returns>
        public static DiffResult[] DiffChar(string textA, string textB, DiffOption option)
        {
            if (string.IsNullOrEmpty(textA) || string.IsNullOrEmpty(textB))
                return StringNullOrEmpty(textA, textB);

            FastDiff diff = new FastDiff();
            if (textA.Length <= textB.Length) {
                diff.SplitChar(textA, textB, option);
            }
            else {
                diff.isSwap = true;
                diff.SplitChar(textB, textA, option);
            }
            return diff.DetectDiff();
        }


        private static DiffResult[] StringNullOrEmpty(string textA, string textB)
        {
            int lengthA = (string.IsNullOrEmpty(textA)) ? 0 : textA.Length;
            int lengthB = (string.IsNullOrEmpty(textB)) ? 0 : textB.Length;
            return PresentDiff(new CommonSubsequence(lengthA, lengthB, 0, null), true);
        }

        private void SplitChar(string textA, string textB, DiffOption option)
        {
            this.dataA = SplitChar(textA, option);
            this.dataB = SplitChar(textB, option);
        }

        private static int[] SplitChar(string text, DiffOption option)
        {
            if (option.IgnoreCase)
                text = text.ToUpperInvariant();

            // TODO: FIXME! Optimize this
            if (option.IgnoreSpace)
                text = Regex.Replace(text, @"\s+", " ");

            if (option.TrimSpace)
                text = text.Trim();

            int[] result = new int[text.Length];
            for (int i = 0; i < text.Length; i++)
                result[i] = text[i];
            return result;
        }

        private void SplitHash(string textA, string textB, DiffOption option)
        {
            if (option.TrimSpace || option.IgnoreSpace) {
                this.dataA = SplitHash(textA, option);
                this.dataB = SplitHash(textB, option);
            }
            else {
                this.dataA = SplitHash(textA, option.IgnoreCase);
                this.dataB = SplitHash(textB, option.IgnoreCase);
            }
        }

        private static int[] SplitHash(string text, bool ignoreCase)
        {
            if (ignoreCase)
                text = text.ToUpperInvariant();

            List<int> list = new List<int>();
            //const int seed = 1315423911;
            const int seed = 5381;
            int h = seed;
            for (int i = 0; i < text.Length; i++) {
                if (text[i] == '\n') {
                    list.Add(h);
                    h = seed;
                    continue;
                }
                //h = ( h << 5 ) + h + text[ i ];
                h = ((h << 5) ^ (h >> 27)) ^ text[i];
            }
            if ((h != seed) || (text[text.Length - 1] == '\n'))
                list.Add(h);

            return list.ToArray();
        }

        private static int[] SplitHash(string text, DiffOption option)
        {
            if (option.IgnoreCase)
                text = text.ToUpperInvariant();

            string[] lines = text.Split('\n');

            // TODO: FIXME! Optimize this
            if (option.IgnoreSpace)
                for (int i = 0; i < lines.Length; ++i)
                    lines[i] = Regex.Replace(lines[i], @"\s+", " ");

            if (option.TrimSpace)
                for (int i = 0; i < lines.Length; ++i)
                    lines[i] = lines[i].Trim();

            int[] hashs = new int[lines.Length];

            for (int i = 0; i < lines.Length; ++i)
                hashs[i] = lines[i].GetHashCode();

            return hashs;
        }

        private DiffResult[] DetectDiff()
        {
            Debug.Assert(this.dataA.Length <= this.dataB.Length);

            this.fp = new Snake[this.dataA.Length + this.dataB.Length + 3];
            int d = this.dataB.Length - this.dataA.Length;
            int p = 0;
            do {
                //Debug.Unindent();
                //Debug.WriteLine( "p:" + p );
                //Debug.Indent();

                for (int k = -p; k < d; k++)
                    SearchSnake(k);

                for (int k = d + p; k >= d; k--)
                    SearchSnake(k);

                p++;
            }
            while (this.fp[this.dataB.Length + 1].posB != (this.dataB.Length + 1));

            // 末尾検出用のCS
            CommonSubsequence endCS = new CommonSubsequence(this.dataA.Length, this.dataB.Length, 0, this.fp[this.dataB.Length + 1].CS);
            CommonSubsequence result = CommonSubsequence.Reverse(endCS);

            if (this.isSwap)
                return PresentDiffSwap(result, true);
            else
                return PresentDiff(result, true);
        }

        private void SearchSnake(int k)
        {
            int kk = this.dataA.Length + 1 + k;
            CommonSubsequence previousCS = null;
            int posA = 0, posB = 0;

            int lk = kk - 1;
            int rk = kk + 1;

            // 論文のfp[n]は-1始まりだが、0始まりのほうが初期化の都合がよいため、
            // +1のゲタを履かせる。fpから読む際は-1し、書く際は+1する。
            int lb = this.fp[lk].posB;
            int rb = this.fp[rk].posB - 1;

            //Debug.Write( "fp[" + string.Format( "{0,2}", k ) + "]=Snake( " + string.Format( "{0,2}", k )
            //    + ", max( fp[" + string.Format( "{0,2}", ( k - 1 ) ) + "]+1= " + string.Format( "{0,2}", lb )
            //    + ", fp[" + string.Format( "{0,2}", ( k + 1 ) ) + "]= " + string.Format( "{0,2}", rb ) + " ))," );

            if (lb > rb) {
                posB = lb;
                previousCS = this.fp[lk].CS;
            }
            else {
                posB = rb;
                previousCS = this.fp[rk].CS;
            }
            posA = posB - k;

            int startA = posA;
            int startB = posB;

            //Debug.Write( "(x: " + string.Format( "{0,2}", startA ) + ", y: " + string.Format( "{0,2}", startB ) + " )" );

            while ((posA < this.dataA.Length)
                && (posB < this.dataB.Length)
                && (this.dataA[posA] == this.dataB[posB])) {
                posA++;
                posB++;
            }

            if (startA != posA) {
                this.fp[kk].CS = new CommonSubsequence(startA, startB, posA - startA, previousCS);
            }
            else {
                this.fp[kk].CS = previousCS;
            }
            this.fp[kk].posB = posB + 1; // fpへ+1して書く。論文のfpに+1のゲタを履かせる。

            //Debug.WriteLine( "= " + string.Format( "{0,2}", posB ) );
        }

        private static DiffResult[] PresentDiff(CommonSubsequence cs, bool wantCommon)
        {
            List<DiffResult> list = new List<DiffResult>();
            int originalStart = 0, modifiedStart = 0;

            while (true) {
                if (originalStart < cs.StartA
                    || modifiedStart < cs.StartB) {
                    DiffResult d = new DiffResult(
                        true,
                        originalStart, cs.StartA - originalStart,
                        modifiedStart, cs.StartB - modifiedStart);
                    list.Add(d);
                }

                // 末尾検出
                if (cs.Length == 0) break;

                originalStart = cs.StartA;
                modifiedStart = cs.StartB;

                if (wantCommon) {
                    DiffResult d = new DiffResult(
                        false,
                        originalStart, cs.Length,
                        modifiedStart, cs.Length);
                    list.Add(d);
                }
                originalStart += cs.Length;
                modifiedStart += cs.Length;

                cs = cs.Next;
            }
            return list.ToArray();
        }

        private static DiffResult[] PresentDiffSwap(CommonSubsequence cs, bool wantCommon)
        {
            List<DiffResult> list = new List<DiffResult>();
            int originalStart = 0, modifiedStart = 0;

            while (true) {
                if (originalStart < cs.StartB
                    || modifiedStart < cs.StartA) {
                    DiffResult d = new DiffResult(
                        true,
                        originalStart, cs.StartB - originalStart,
                        modifiedStart, cs.StartA - modifiedStart);
                    list.Add(d);
                }

                // 末尾検出
                if (cs.Length == 0) break;

                originalStart = cs.StartB;
                modifiedStart = cs.StartA;

                if (wantCommon) {
                    DiffResult d = new DiffResult(
                        false,
                        originalStart, cs.Length,
                        modifiedStart, cs.Length);
                    list.Add(d);
                }
                originalStart += cs.Length;
                modifiedStart += cs.Length;

                cs = cs.Next;
            }
            return list.ToArray();
        }

        private struct Snake
        {
            public int posB;
            public CommonSubsequence CS;

            public override string ToString()
            {
                return "posB:" + this.posB + ", CS:" + ((this.CS == null) ? "null" : "exist");
            }
        }

        private class CommonSubsequence
        {
            private int startA_, startB_;
            private int length_;
            public CommonSubsequence Next;

            public int StartA { get { return this.startA_; } }
            public int StartB { get { return this.startB_; } }
            public int Length { get { return this.length_; } }

            public CommonSubsequence() { }

            public CommonSubsequence(int startA, int startB, int length, CommonSubsequence next)
            {
                this.startA_ = startA;
                this.startB_ = startB;
                this.length_ = length;
                this.Next = next;
            }

            public static CommonSubsequence Reverse(CommonSubsequence old)
            {
                CommonSubsequence newTop = null;
                while (old != null) {
                    CommonSubsequence next = old.Next;
                    old.Next = newTop;
                    newTop = old;
                    old = next;
                }
                return newTop;
            }

            public override string ToString()
            {
                return "Length:" + this.Length + ", A:" + this.StartA.ToString()
                    + ", B:" + this.StartB.ToString() + ", Next:" + ((this.Next == null) ? "null" : "exist");
            }
        }

    }
}