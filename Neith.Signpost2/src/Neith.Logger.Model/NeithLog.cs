using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace Neith.Logger.Model
{
    [Serializable]
    public class NeithLog : ReactiveValidatedObject, IEquatable<NeithLog>
    {
        #region フィールド
        /// <summary>タイムスタンプ(UTC時刻、IDを兼ねるためUnique保証)</summary>
        [Key]
        public DateTimeOffset UtcTime { get { return _UtcTime; } set { this.RaiseAndSetIfChanged(a => a.UtcTime, value); } }
        private DateTimeOffset _UtcTime;

        /// <summary>ログの収集モジュール</summary>
        public string Collector { get { return _Collector; } set { this.RaiseAndSetIfChanged(a => a.Collector, value); } }
        private string _Collector;

        /// <summary>ホスト</summary>
        public string Host { get { return _Host; } set { this.RaiseAndSetIfChanged(a => a.Host, value); } }
        private string _Host;

        /// <summary>プロセスID</summary>
        public int Pid { get { return _Pid; } set { this.RaiseAndSetIfChanged(a => a.Pid, value); } }
        private int _Pid;

        /// <summary>ログの取得元アプリ</summary>
        public string Application { get { return _Application; } set { this.RaiseAndSetIfChanged(a => a.Application, value); } }
        private string _Application;

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain { get { return _Domain; } set { this.RaiseAndSetIfChanged(a => a.Domain, value); } }
        private string _Domain;

        /// <summary>ログの取得元ユーザ</summary>
        public string User { get { return _User; } set { this.RaiseAndSetIfChanged(a => a.User, value); } }
        private string _User;

        /// <summary>ログ内容</summary>
        public string LogText { get { return _LogText; } set { this.RaiseAndSetIfChanged(a => a.LogText, value); } }
        private string _LogText;

        /// <summary>ログの分析モジュール</summary>
        public string Analyzer { get { return _Analyzer; } set { this.RaiseAndSetIfChanged(a => a.Analyzer, value); } }
        private string _Analyzer;

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd { get { return _HWnd; } set { this.RaiseAndSetIfChanged(a => a.HWnd, value); } }
        private IntPtr _HWnd;

        /// <summary>ログのカテゴリ</summary>
        public string Category { get { return _Category; } set { this.RaiseAndSetIfChanged(a => a.Category, value); } }
        private string _Category;

        /// <summary>ログのタイプ</summary>
        public string Type { get { return _Type; } set { this.RaiseAndSetIfChanged(a => a.Type, value); } }
        private string _Type;

        /// <summary>ログの優先度</summary>
        public NeithLogPriority Priority { get { return _Priority; } set { this.RaiseAndSetIfChanged(a => a.Priority, value); } }
        private NeithLogPriority _Priority;

        /// <summary>行為の実行者</summary>
        public string Actor { get { return _Actor; } set { this.RaiseAndSetIfChanged(a => a.Actor, value); } }
        private string _Actor;

        /// <summary>行為の対象</summary>
        public string Target { get { return _Target; } set { this.RaiseAndSetIfChanged(a => a.Target, value); } }
        private string _Target;

        /// <summary>解析結果メッセージ</summary>
        public string Message { get { return _Message; } set { this.RaiseAndSetIfChanged(a => a.Message, value); } }
        private string _Message;

        /// <summary>アイコン画像URL</summary>
        public string Icon { get { return _Icon; } set { this.RaiseAndSetIfChanged(a => a.Icon, value); } }
        private string _Icon;

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