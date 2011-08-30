using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neith.Logger.Parsers
{
    /// <summary>
    /// パケット読込インターフェース。
    /// </summary>
    public interface IPacketReader
    {
        /// <summary>
        /// 指定されたバッファにパケットを読み込み、結果を返します。
        /// 1byte以上のデータを返すことを保証します。
        /// </summary>
        /// <param name="buffer">バッファ</param>
        /// <returns>読み込んだデータ。</returns>
        Task<ArraySegment<byte>> ReadAsync(ArraySegment<byte> buffer);


    }
}
