using System;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Logger.Model
{
    public interface INeithNotification : INotification, IEquatable<INeithNotification>
    {
        /// <summary>データ本体</summary>
        NeithNotificationRec Rec { get; }

        /// <summary>イベント発生時刻</summary>
        DateTimeOffset EventTime { get; set; }

        /// <summary>プロセスID</summary>
        int Pid { get; set; }

        /// <summary>ログの取得元ドメイン</summary>
        string Domain { get; set; }

        /// <summary>ログの取得元ユーザ</summary>
        string User { get; set; }

        /// <summary>ログ内容</summary>
        string LogText { get; set; }

        /// <summary>WindowHandle</summary>
        IntPtr HWnd { get; set; }

        /// <summary>ログのカテゴリ</summary>
        string Category { get; set; }

        /// <summary>ログのタイプ</summary>
        string Type { get; set; }

        /// <summary>行為の対象</summary>
        string Target { get; set; }
    }
}