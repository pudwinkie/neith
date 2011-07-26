using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using ReactiveUI;

namespace Neith.Logger.Model
{
    /// <summary>
    /// ログモデル。
    /// </summary>
    public class NeithLogModel : ReactiveValidatedObject, IEquatable<NeithLogModel>
    {
        /// <summary>データ本体</summary>
        public NeithLog Data { get; private set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="data"></param>
        public NeithLogModel(NeithLog data)
        {
            Data = data;
        }

        #region プロパティ
        /// <summary>タイムスタンプ(UTC時刻、IDを兼ねるためUnique保証)</summary>
        public DateTimeOffset UtcTime { get { return Data.UtcTime; } set { this.RaiseAndSetIfChanged(a => a.UtcTime, ref Data.UtcTime, value); } }

        /// <summary>ログの収集モジュール</summary>
        public string Collector { get { return Data.Collector; } set { this.RaiseAndSetIfChanged(a => a.Collector, ref Data.Collector, value); } }

        /// <summary>ホスト</summary>
        public string Host { get { return Data.Host; } set { this.RaiseAndSetIfChanged(a => a.Host, ref Data.Host, value); } }

        /// <summary>プロセスID</summary>
        public int Pid { get { return Data.Pid; } set { this.RaiseAndSetIfChanged(a => a.Pid, ref Data.Pid, value); } }

        /// <summary>ログの取得元アプリ</summary>
        public string Application { get { return Data.Application; } set { this.RaiseAndSetIfChanged(a => a.Application, ref Data.Application, value); } }

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain { get { return Data.Domain; } set { this.RaiseAndSetIfChanged(a => a.Domain, ref Data.Domain, value); } }

        /// <summary>ログの取得元ユーザ</summary>
        public string User { get { return Data.User; } set { this.RaiseAndSetIfChanged(a => a.User, ref Data.User, value); } }

        /// <summary>ログ内容</summary>
        public string LogText { get { return Data.LogText; } set { this.RaiseAndSetIfChanged(a => a.LogText, ref Data.LogText, value); } }

        /// <summary>ログの分析モジュール</summary>
        public string Analyzer { get { return Data.Analyzer; } set { this.RaiseAndSetIfChanged(a => a.Analyzer, ref Data.Analyzer, value); } }

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd { get { return Data.HWnd; } set { this.RaiseAndSetIfChanged(a => a.HWnd, ref Data.HWnd, value); } }

        /// <summary>ログのカテゴリ</summary>
        public string Category { get { return Data.Category; } set { this.RaiseAndSetIfChanged(a => a.Category, ref Data.Category, value); } }

        /// <summary>ログのタイプ</summary>
        public string Type { get { return Data.Type; } set { this.RaiseAndSetIfChanged(a => a.Type, ref Data.Type, value); } }

        /// <summary>ログの優先度</summary>
        public NeithLogPriority Priority { get { return Data.Priority; } set { this.RaiseAndSetIfChanged(a => a.Priority, ref Data.Priority, value); } }

        /// <summary>行為の実行者</summary>
        public string Actor { get { return Data.Actor; } set { this.RaiseAndSetIfChanged(a => a.Actor, ref Data.Actor, value); } }

        /// <summary>行為の対象</summary>
        public string Target { get { return Data.Target; } set { this.RaiseAndSetIfChanged(a => a.Target, ref Data.Target, value); } }

        /// <summary>解析結果メッセージ</summary>
        public string Message { get { return Data.Message; } set { this.RaiseAndSetIfChanged(a => a.Message, ref Data.Message, value); } }

        /// <summary>アイコン画像URL</summary>
        public string Icon { get { return Data.Icon; } set { this.RaiseAndSetIfChanged(a => a.Icon, ref Data.Icon, value); } }

        #endregion
        #region メソッド
        public static NeithLogModel Create()
        {
            return new NeithLogModel(NeithLog.Create());
        }

        public override string ToString()
        {
            return Data.ToString();
        }

        #endregion

        public bool Equals(NeithLogModel other)
        {
            if (other == null) return false;
            return Data.Equals(other.Data);
        }

    }
}
