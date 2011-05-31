using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Neith.Logger.Model
{
    [Serializable]
    public struct NeithLog : IEquatable<NeithLog>
    {
        #region フィールド
        /// <summary>タイムスタンプ(UTC時刻、IDを兼ねるためUnique保証)</summary>
        public DateTime UtcTime;

        /// <summary>ログの収集モジュール</summary>
        public string Collector;

        /// <summary>ホスト</summary>
        public string Host;

        /// <summary>プロセスID</summary>
        public int Pid;

        /// <summary>ログの取得元アプリ</summary>
        public string Application;

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain;

        /// <summary>ログの取得元ユーザ</summary>
        public string User;

        /// <summary>ログ内容</summary>
        public string LogText;

        /// <summary>ログの分析モジュール</summary>
        public string Analyzer { get; set; }

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd { get; set; }

        /// <summary>ログのカテゴリ</summary>
        public string Category { get; set; }

        /// <summary>ログのタイプ</summary>
        public string Type { get; set; }

        /// <summary>ログの優先度</summary>
        public NeithLogPriority Priority { get; set; }

        /// <summary>行為の実行者</summary>
        public string Actor { get; set; }

        /// <summary>行為の対象</summary>
        public string Target { get; set; }

        /// <summary>解析結果メッセージ</summary>
        public string Message { get; set; }

        /// <summary>アイコン画像URL</summary>
        public string Icon { get; set; }

        #endregion
        #region メソッド
        public static NeithLog Create()
        {
            var log = new NeithLog();
            log.UtcTime = UniqueTime.Now;
            return log;
        }

        public override string ToString()
        {
            return string.Format("{0:O}: {1}", UtcTime.ToLocalTime(), Message);
        }

        #endregion

        public bool Equals(NeithLog other)
        {
            return this.UtcTime == other.UtcTime
                && this.Collector == other.Collector
                && this.Host == other.Host
                && this.Pid == other.Pid
                && this.Application == other.Application
                && this.Domain == other.Domain
                && this.User == other.User
                && this.LogText == other.LogText
                && this.Analyzer == other.Analyzer
                ;
        }

    }
}