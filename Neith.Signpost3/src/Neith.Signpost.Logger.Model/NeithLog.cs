using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Logger.Model
{
    /// <summary>保管ログ</summary>
    public class NeithLog
    {
        /// <summary>発生時刻</summary>
        public DateTime Time { get; set; }

        /// <summary>ログ生成アプリ</summary>
        public string Application { get; set; }


        /// <summary>行為の実行者</summary>
        public string Sender { get; set; }

        /// <summary>実行者のグループ</summary>
        public string SenderGroup { get; set; }



        /// <summary>行為名</summary>
        public string Title { get; set; }

        /// <summary>行為の詳細</summary>
        public string Body { get; set; }



        /// <summary>行為対象</summary>
        public string Target { get; set; }

        /// <summary>行為対象のグループ</summary>
        public string TargetGroup { get; set; }

        /// <summary>影響を受ける属性</summary>
        public string Property { get; set; }

        /// <summary>その他、影響を受ける属性</summary>
        public string[] MoreAttributes { get; set; }

        /// <summary>値</summary>
        public object Value { get; set; }

        /// <summary>値の更新モード</summary>
        public ValueUpdateMode ValueUpdateMode { get; set; }



        /// <summary>親通知が存在する場合</summary>
        public NeithLog Parent { get; set; }

        /// <summary>ログのオリジナルソース</summary>
        public object Source { get; set; }

    }
}
