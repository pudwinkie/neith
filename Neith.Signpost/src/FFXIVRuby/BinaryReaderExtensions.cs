using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FFXIVRuby
{
    public static class BinaryReaderExtensions
    {
        /// <summary>
        /// バイナリ配列より、32bit値を列挙します。
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        public static IEnumerable<int> EnReadInt32(this byte[] buf)
        {
            for (int i = 0; i < (buf.Length - 3); i += 4) {
                yield return BitConverter.ToInt32(buf, i);
            }
        }

    }
}
