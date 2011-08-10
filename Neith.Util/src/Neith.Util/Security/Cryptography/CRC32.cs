using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util.Security.Cryptography
{
    /// <summary>
    /// CRC32を算出するためのクラス。
    /// </summary>
    public class CRC32
    {
        #region CRCオブジェクト
        /// <summary> CRC値を取得します.</summary>
        public uint Value { get { return crc; } }
        private uint crc = 0;

        /// <summary>
        /// CRC値をbyte配列で更新します。
        /// </summary>
        /// <param name="buf">CRC計算するbyte配列</param>
        /// <param name="off">データの開始位置を示す配列上のインデックス</param>
        /// <param name="len">実際に計算するデータのバイト数</param>
        /// <returns>新しいCRC値</returns>
        public uint Update(byte[] buf, int off, int len)
        {
            crc = Calc(crc, buf, off, len);
            return crc;
        }

        /// <summary>
        /// CRCの値をbyte配列で更新します。
        /// </summary>
        /// <param name="b">CRCを計算するbyte配列</param>
        /// <returns>新しいCRC値</returns>
        public uint Update(byte[] b)
        {
            return Update(b, 0, b.Length);
        }

        /// <summary> 
        /// CRC値をbyte値で更新します。
        /// </summary>
        /// <param name="b">CRCを計算するデータ</param>
        public uint Update(byte b)
        {
            return Update(new byte[] { b });
        }

        /// <summary> 
        /// CRC値を０にリセットします
        /// </summary>
        public void Reset()
        {
            crc = 0;
        }
        #endregion
        #region CRC計算ロジック本体

        /// <summary>
        /// 計算用テーブル。
        /// </summary>
        private static readonly uint[] CRC32_TABLE = InitCrcTable();
        private static uint[] InitCrcTable()
        {
            uint[] table = new uint[256];
            for (uint n = 0; n < 256; n++) {
                uint c = n;
                for (uint k = 0; k < 8; k++) {
                    c = (((c & 1) != 0) ? (0xedb88320U) : 0) ^ (c >> 1);
                }
                table[n] = c;
            }
            return table;
        }

        /// <summary>
        /// CRC計算を行います。
        /// </summary>
        /// <param name="nCrcBegin"></param>
        /// <param name="buf"></param>
        /// <param name="off"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static uint Calc(uint nCrcBegin, byte[] buf, int off, int len)
        {
            uint nCrc = nCrcBegin ^ 0xFFFFFFFF;
            int end = (off + len);
            for (int i = off; i < end; i++) {
                nCrc = CRC32_TABLE[(nCrc ^ buf[i]) & 0xFF] ^ (nCrc >> 8);
            }
            return (nCrc ^ 0xFFFFFFFF);
        }

        #endregion
    }
}
