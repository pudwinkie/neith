using System;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Logger.Model
{
    public interface INeithNotification : INotification
    {
        /// <summary>データ本体</summary>
        NeithNotificationRec Rec { get; }

        /// <summary>イベント発生時刻</summary>
        DateTimeOffset EventTime { get; set; }

        /// <summary>プロセスへのヒモ付情報</summary>
        string ProcessKey { get; set; }

        /// <summary>ログの取得元ユーザ</summary>
        string User { get; set; }

        /// <summary>ログ内容</summary>
        string LogText { get; set; }

        /// <summary>ログのカテゴリ</summary>
        string Category { get; set; }

        /// <summary>ログのタイプ</summary>
        string Type { get; set; }

        /// <summary>行為の対象</summary>
        string Target { get; set; }
    }
}