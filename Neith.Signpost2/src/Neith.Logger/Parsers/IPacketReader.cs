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
        /// </summary>
        /// <param name="buffers">バッファ</param>
        /// <param name="langth">最大読み込みサイズ</param>
        /// <returns>読み込んだバイト数。</returns>
        Task<int> ReceiveTaskAsync(IList<ArraySegment<byte>> buffers, int langth);
    }
}
