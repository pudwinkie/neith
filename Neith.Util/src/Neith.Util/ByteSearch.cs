using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{
    /// <summary>
    /// バイナリサーチ。
    /// </summary>
    public class ByteSearch
    {
        private readonly byte[] _pattern;
        private readonly int[] _skipArray = new int[256];

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="pattern"></param>
        public ByteSearch(byte[] pattern)
        {
            _pattern = pattern;
            for (int i = 0; i < _skipArray.Length; i++)
                _skipArray[i] = _pattern.Length;
            for (int i = 0; i < _pattern.Length - 1; i++)
                _skipArray[_pattern[i]] = _pattern.Length - i - 1;
        }

        /// <summary>
        /// パターンが最初に表れる位置を返します。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int IndexOf<TList>(TList target, int offset, int count)
            where TList : IList<byte>
        {
            int i = offset;
            var length = offset + count;

            // Loop while there's still room for search term
            while (i <= (length - _pattern.Length)) {
                // Look if we have a match at this position
                int j = _pattern.Length - 1;
                while (j >= 0 && _pattern[j] == target[i + j])
                    j--;

                if (j < 0) {
                    // Match found
                    return i;
                }

                // Advance to next comparision
                i += Math.Max(_skipArray[target[i + j]] - _pattern.Length + 1 + j, 1);
            }
            // No match found
            return -1;
        }

        /// <summary>
        /// パターンが最初に表れる位置を返します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int IndexOf<TList>(TList target)
            where TList : IList<byte>
        {
            return IndexOf(target, 0, target.Count);
        }

        /// <summary>
        /// パターンが最初に表れる位置を返します。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int IndexOf(ArraySegment<byte> target)
        {
            return IndexOf(target.Array, target.Offset, target.Count);
        }

    }
}
