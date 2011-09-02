using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.GNTP.Parsers
{
    /// <summary>
    /// パーサ：ステージ３。意味解析を行なう。
    /// 暗号キーの認証も行なう。
    /// </summary>
    public class Stage3
    {
        private readonly Stage2 Reader;

        public Stage3(Stage2 stage2Reader)
        {
            Reader = stage2Reader;
        }

        /// <summary>
        /// ヘッダを解釈します。暗号
        /// </summary>
        public async void ReadHeader()
        {
            var header = await Reader.ReadHeader();




        }



    }
}
