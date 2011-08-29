using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Parsers
{
    /// <summary>
    /// パーサ部品
    /// </summary>
    public static class ParserUtil
    {
        private const byte CR = 0x0d;
        private const byte LF = 0x0a;
        private const int INVALID_INDEX = -1;
        private static readonly ArraySegment<byte> ZERO_SEG = new ArraySegment<byte>(new byte[0]);

        /// <summary>
        /// CRLFを検索します。発見した場合はLFのoffset位置を返します。
        /// 見つからない場合は-1を返します。
        /// </summary>
        /// <param name="seg"></param>
        /// <returns></returns>
        public static int IndexOfCRLF(this ArraySegment<byte> seg)
        {
            var src = seg.Array;
            var index = seg.Offset + 1;
            var stopIndex = index + seg.Count;
            while (index < stopIndex) {
                switch (src[index]) {
                    case LF:
                        if (src[index - 1] == CR) return index;
                        index += 2;
                        continue;
                    case CR:
                        index++;
                        continue;
                }
                index += 2;
            }
            return INVALID_INDEX;
        }

        /// <summary>
        /// resultBufferの末尾がCRLFになるようにdataを追加します。
        /// </summary>
        /// <param name="buffer">バッファ</param>
        /// <param name="seg">追加するデータ</param>
        /// <param name="remain">末尾がCRLFの場合の残り</param>
        /// <returns>末尾がCRLFならtrue</returns>
        public static bool ScanCRLF(IList<byte[]> buffer, ArraySegment<byte> seg, out  ArraySegment<byte> remain)
        {
            remain = ZERO_SEG;
            if (seg.Count == 0) return false;
            if (buffer.Count > 0) {
                var lastBuf = buffer[buffer.Count - 1];
                if (lastBuf[lastBuf.Length - 1] == CR && seg.Count > 0 && seg.Array[seg.Offset] == LF) {
                    return ScanCRLF_Match(buffer, seg, 1, out remain);
                }
            }
            var index = IndexOfCRLF(seg);
            if (index < 0) {
                buffer.Add(seg.ToArray());
                return false;
            }
            return ScanCRLF_Match(buffer, seg, index - seg.Offset + 1, out remain);
        }

        private static bool ScanCRLF_Match(IList<byte[]> buffer, ArraySegment<byte> seg, int size, out ArraySegment<byte> remain)
        {
            var addSeg = new ArraySegment<byte>(seg.Array, seg.Offset, size);
            remain = new ArraySegment<byte>(seg.Array, seg.Offset + size, seg.Count - size);
            buffer.Add(addSeg.ToArray());
            return true;
        }



    }
}
