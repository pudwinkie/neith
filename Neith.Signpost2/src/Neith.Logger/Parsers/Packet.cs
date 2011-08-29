using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Parsers
{
    /// <summary>
    /// 解析単位パケット。１行又は１区切りのバイトデータ。
    /// </summary>
    public class Packet
    {
        /// <summary>byte配列の列挙</summary>
        public IEnumerable<ArraySegment<byte>> ByteSegments { get; private set; }

        /// <summary>文字情報</summary>
        public string Text { get; private set; }

        /// <summary>文字情報ならtrue</summary>
        public bool IsText { get { return Text != null; } }

        /// <summary>byte配列の連結結果</summary>
        public byte[] Bytes { get { return LazyBytes.Value; } }
        private readonly Lazy<byte[]> LazyBytes;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="segs"></param>
        /// <param name="text"></param>
        public Packet(IEnumerable<ArraySegment<byte>> segs, string text = null)
        {
            ByteSegments = segs;
            Text = text;
            LazyBytes = new Lazy<byte[]>(() => ByteSegments.ToCombineArray());
        }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="text"></param>
        public Packet(string text)
            : this(null, text)
        {
        }

    }

}