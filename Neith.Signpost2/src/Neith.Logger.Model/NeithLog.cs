using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Wintellect.Sterling;

namespace Neith.Logger.Model
{
    [Serializable]
    public class NeithLog : IEquatable<NeithLog>
    {
        #region フィールド(Growl互換[IExtensibleObject])

        /// <summary>マシン名</summary>
        public string MachineName;

        /// <summary>プラットフォーム(OS)名</summary>
        public string PlatformName;

        /// <summary>プラットフォーム(OS)バージョン</summary>
        public string PlatformVersion;

        /// <summary>接続フレームワーク名</summary>
        public string SoftwareName;

        /// <summary>接続フレームワーク名</summary>
        public string SoftwareVersion;



        #endregion
        #region フィールド(Signpost拡張)
        /// <summary>タイムスタンプ(UTC時刻、IDを兼ねるためUnique保証)</summary>
        [Key]
        public DateTimeOffset UtcTime;

        /// <summary>ログの収集モジュール</summary>
        public string Collector;

        /// <summary>プロセスID</summary>
        public int Pid;

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain;

        /// <summary>ログの取得元ユーザ</summary>
        public string User;

        /// <summary>ログ内容</summary>
        public string LogText;

        /// <summary>ログの分析モジュール</summary>
        public string Analyzer;

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd;

        /// <summary>ログのカテゴリ</summary>
        public string Category;

        /// <summary>ログのタイプ</summary>
        public string Type;

        /// <summary>ログの優先度</summary>
        public NeithLogPriority Priority;

        /// <summary>行為の実行者</summary>
        public string Actor;

        /// <summary>行為の対象</summary>
        public string Target;

        /// <summary>解析結果メッセージ</summary>
        public string Text;

        /// <summary>アイコン画像URL</summary>
        public string Icon;

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
            return string.Format("{0:O}: {1}", UtcTime.ToLocalTime(), Text);
        }

        #endregion

        public bool Equals(NeithLog other)
        {
            return this.UtcTime == other.UtcTime
                && this.Collector == other.Collector
                && this.MachineName == other.MachineName
                && this.Pid == other.Pid
                && this.SoftwareName == other.SoftwareName
                && this.Domain == other.Domain
                && this.User == other.User
                && this.LogText == other.LogText
                && this.Analyzer == other.Analyzer
                ;
        }
    }
}