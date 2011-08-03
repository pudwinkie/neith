using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    public static class NeithHeaderKeys
    {
        /// <SUMMARY>ヘッダプリフィックス</SUMMARY>
        internal const string PREFIX = "X-Neith-";

        /// <SUMMARY>イベント発生時刻</SUMMARY>
        public const string EVENT_TIME = PREFIX + "Event-Time";

        /// <SUMMARY>プロセスへのひも付け情報</SUMMARY>
        public const string PROCESS_KEY = PREFIX + "Process";

        /// <SUMMARY>ログの取得元サーバ</SUMMARY>
        public const string SERVER = PREFIX + "Server";

        /// <SUMMARY>ログの取得元ユーザ</SUMMARY>
        public const string USER = PREFIX + "User";

        /// <SUMMARY>オリジナルログの内容</SUMMARY>
        public const string LOGTEXT = PREFIX + "Log-Text";

        /// <SUMMARY>ログのカテゴリ</SUMMARY>
        public const string CATEGORY = PREFIX + "Category";

        /// <SUMMARY>ログのタイプ</SUMMARY>
        public const string TYPE = PREFIX + "Type";

        /// <SUMMARY>行為の対象</SUMMARY>
        public const string TARGET = PREFIX + "Target";
    }
}