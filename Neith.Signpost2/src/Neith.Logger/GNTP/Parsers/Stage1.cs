using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neith.Growl.Connector;
using Neith.Util;

namespace Neith.Logger.GNTP.Parsers
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
        private static readonly ByteSearch ScanCRLFCRLF
            = new ByteSearch(new byte[] { ParserUtil.CR, ParserUtil.LF, ParserUtil.CR, ParserUtil.LF });

        private readonly ArraySegment<byte> BufferL;
        private readonly ArraySegment<byte> BufferS;
        private readonly ArraySegment<byte> ZeroBuffer;
        private ArraySegment<byte> BufferRemain { get; set; }

        public IPacketReader Reader { get; private set; }

        public Key Key { get; set; }

        public byte[] IV { get; set; }

        /// <summary>平文ならtrue</summary>
        public bool IsPlainText { get { return Key.EncryptionAlgorithm == Cryptography.SymmetricAlgorithmType.PlainText; } }

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
            if (count == 0) ClearRemain();
            else BufferRemain = new ArraySegment<byte>(array, offset, count);
        }


        /// <summary>
        /// ヘッダ行（平文）を検出して読み出す。
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadHeader()
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


        /// <summary>
        /// 改行を検出。改行でなければInvalidCharException。
        /// </summary>
        /// <exception cref="InvalidCharException">改行コードではない</exception>
        /// <returns></returns>
        public async void ReadCRLF()
        {
            var count = 0;
            while (true) {
                var index = 0;
                var read = await ReadS();
                if (count == 0) {
                    if (ParserUtil.CR != read.ElementAt(index))
                        throw new InvalidCharException();
                    index++;
                    count++;
                }
                if (count == 1 && read.Count > index) {
                    if (ParserUtil.LF != read.ElementAt(index))
                        throw new InvalidCharException();
                    UpdateRemain(index + 1);
                    return;
                }
                ClearRemain();
            }
        }

        /// <summary>
        /// 空改行で終わるテキストブロックを読み込みます。
        /// </summary>
        /// <returns></returns>
        public async Task<string> ReadTextBlock()
        {
            if (IsPlainText) return await ReadTextBlockPlain();
            else return await ReadTextBlockEncryption();
        }

        /// <summary>
        /// 指定長のバイナリ領域を読み込みます。
        /// </summary>
        /// <returns></returns>
        public async Task<byte[]> ReadBinBlock(int count)
        {
            if (IsPlainText) return await ReadBinBlockPlain(count);
            else return await ReadBinBlockEncryption(count);
        }

        #region 平文の読み込み処理

        /// <summary>
        /// [CRLF][CRLF]を検出するまで読み込み、テキストとして出力します。
        /// </summary>
        /// <returns></returns>
        private async Task<string> ReadTextBlockPlain()
        {
            var textBuffer = new List<byte[]>();
            var target = new List<ArraySegment<byte>>();
            var targetCount = 0;
            while (true) {
                // ターゲットが4byteを超えるまで読み込む。
                var read = await ReadL();
                targetCount += read.Count;
                if (targetCount < 4) {
                    var a = new ArraySegment<byte>(read.ToArray());
                    target.Add(a);
                    ClearRemain();
                    continue;
                }
                target.Add(read);
                // 検索と発見時の処理
                var index = ScanCRLFCRLF.IndexOf(target.ToList());
                if (index >= 0) {
                    var count = index + 4;
                    textBuffer.Add(target.ToCombineArray(count));
                    UpdateRemain(count - (targetCount - read.Count));
                    break;
                }
                // 発見できなければ最後の3byteを切りだして次の検索へ
                target.Clear();
                var tail = new byte[3];
                Buffer.BlockCopy(read.Array, read.Offset + read.Count - 3, tail, 0, 3);
                target.Add(new ArraySegment<byte>(tail));
                targetCount = 3;
                ClearRemain();
            }
            return Encoding.UTF8.GetString(textBuffer.ToCombineArray());
        }

        /// <summary>
        /// 固定ブロックの読み込み
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private async Task<byte[]> ReadBinBlockPlain(int count)
        {
            var textBuffer = new List<byte[]>();
            while (count > 0) {
                ArraySegment<byte> read;
                if (count > BUFFER_SIZE_S) read = await ReadL();
                else read = await ReadS();
                count -= read.Count;
                if (count >= 0) {
                    textBuffer.Add(read.ToArray());
                    ClearRemain();
                }
                else {
                    var readCount = read.Count + count;
                    textBuffer.Add(read.ToArray(readCount));
                    UpdateRemain(readCount);
                }
            }
            return textBuffer.ToCombineArray();
        }


        #endregion
        #region 暗号の読み込み処理

        private async Task<string> ReadTextBlockEncryption()
        {
            var textBuffer = new List<byte>();
            var target = new byte[16];
            var targetOffset = 0;
            while (true) {
                // 16byte読み込む
                var read = await ReadS();
                var readCount = read.Count;
                if (readCount > 16 - targetOffset) readCount = 16 - targetOffset;
                Buffer.BlockCopy(read.Array, read.Offset, target, targetOffset, readCount);
                targetOffset += readCount;
                UpdateRemain(readCount);
                if (targetOffset < 16) continue;
                // 復号
                var dec = Key.Decrypt(target, IV);
                var index = Array.IndexOf<byte>(dec, 0);
                if (index >=0) {
                    textBuffer.AddRange(dec.Take(index));
                    break;
                }
                textBuffer.AddRange(dec);
                targetOffset = 0;
            }
            return Encoding.UTF8.GetString(textBuffer.ToArray()) + "\r\n\r\n";
        }

        private async Task<byte[]> ReadBinBlockEncryption(int count)
        {
            // データは16byte単位でパディングされるため、サイズの調整
            var readCount = (count / 16 + count % 16 == 0 ? 0 : 1) * 16;
            var read = await ReadBinBlockPlain(readCount);
            var dec = Key.Decrypt(read, IV);
            var seg = new ArraySegment<byte>(dec, 0, count);
            return seg.ToArray();
        }

        #endregion

    }
}
