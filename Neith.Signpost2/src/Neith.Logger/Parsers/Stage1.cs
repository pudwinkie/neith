using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neith.Logger.Parsers
{
    /// <summary>
    /// パーサステージ１。暗号解釈を担当する。
    /// 要求に基づき、バイナリ情報より以下のいずれかのトークンを切り出す。
    /// ■１行要求
    /// 　・１行分の平文テキスト
    /// ■改行要求
    /// 　・改行
    /// ■テキストブロック要求
    /// 　・空改行で終わるテキストブロック
    /// 　・暗号化されたテキストブロック
    /// ■バイナリブロック要求
    /// 　・指定長のバイナリブロック
    /// 　・指定長で暗号化されたバイナリブロック
    /// 
    /// モードとして、平文モードと暗号モードがある。暗号モードの場合、復号して返す。
    /// </summary>
    public class Stage1
    {
        private const int BUFFER_SIZE_L = 4096;
        private const int BUFFER_SIZE_S = 256;
        private static readonly byte[] ARRAY_LF = new byte[] { ParserUtil.LF };

        private readonly ArraySegment<byte> BufferL;
        private readonly ArraySegment<byte> BufferS;
        private readonly ArraySegment<byte> ZeroBuffer;
        private ArraySegment<byte> BufferRemain { get; set; }

        public IPacketReader Reader { get; private set; }

        private Stage1()
        {
            var buf = new byte[BUFFER_SIZE_L];
            BufferL = new ArraySegment<byte>(buf, 0, BUFFER_SIZE_L);
            BufferS = new ArraySegment<byte>(buf, 0, BUFFER_SIZE_S);
            ZeroBuffer = new ArraySegment<byte>(buf, 0, 0);
            ClearRemain();
        }

        public Stage1(IPacketReader reader)
            : this()
        {
            Reader = reader;
        }

        /// <summary>
        /// 小さなバッファで読み込む。
        /// </summary>
        /// <returns></returns>
        private async Task<ArraySegment<byte>> ReadS()
        {
            if (BufferRemain.Count > 0) return BufferRemain;
            var rc = await Reader.ReadAsync(BufferS);
            BufferRemain = rc;
            return rc;
        }

        /// <summary>
        /// 大きなバッファで読み込む。
        /// </summary>
        /// <returns></returns>
        private async Task<ArraySegment<byte>> ReadL()
        {
            if (BufferRemain.Count > 0) return BufferRemain;
            var rc = await Reader.ReadAsync(BufferL);
            BufferRemain = rc;
            return rc;
        }

        /// <summary>
        /// BufferRemainのサイズを０に設定します。
        /// </summary>
        /// <param name="read"></param>
        private void ClearRemain()
        {
            BufferRemain = ZeroBuffer;
        }

        /// <summary>
        /// 渡されたセグメントの長さ分だけ読み込んだものとしてBufferRemainを切り詰める。
        /// </summary>
        /// <param name="read"></param>
        private void UpdateRemain(ArraySegment<byte> read)
        {
            UpdateRemain(read.Count);
        }

        /// <summary>
        /// 渡されたセグメントを読み込んだものとしてBufferRemainを切り詰める。
        /// </summary>
        /// <param name="readCount"></param>
        private void UpdateRemain(int readCount)
        {
            var array = BufferRemain.Array;
            var offset = BufferRemain.Offset + readCount;
            var count = BufferRemain.Count - readCount;
            BufferRemain = new ArraySegment<byte>(array, offset, count);
        }


        /// <summary>
        /// ヘッダ行（平文）を検出して読み出す。
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadHeaderAsync()
        {
            var textBuffer = new List<byte[]>();
            var isLastCR = false;
            while (true) {
                var read = await ReadS();
                if (isLastCR && read.ElementAt(0) == ParserUtil.LF) {
                    textBuffer.Add(ARRAY_LF);
                    UpdateRemain(1);
                    break;
                }
                var crlf = read.IndexOfCRLF();
                if (crlf >= 0) {
                    var count = crlf - read.Offset + 1;
                    textBuffer.Add(read.ToArray(count));
                    UpdateRemain(count);
                    break;
                }
                textBuffer.Add(read.ToArray());
                ClearRemain();
            }
            return Encoding.UTF8.GetString(textBuffer.ToCombineArray());
        }

    }
}
