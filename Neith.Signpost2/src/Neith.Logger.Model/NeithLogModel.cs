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
        public NeithLog Rec { get; private set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="data"></param>
        public NeithLogModel(NeithLog data)
        {
            Rec = data;
        }

        #region プロパティ(Growl互換[IExtensibleObject])
        /// <summary>マシン名</summary>
        public string MachineName { get { return Rec.MachineName; } set { this.RaiseAndSetIfChanged(a => a.MachineName, ref Rec.MachineName, value); } }

        /// <summary>プラットフォーム(OS)名</summary>
        public string PlatformName { get { return Rec.PlatformName; } set { this.RaiseAndSetIfChanged(a => a.PlatformName, ref Rec.PlatformName, value); } }

        /// <summary>プラットフォーム(OS)バージョン</summary>
        public string PlatformVersion { get { return Rec.PlatformVersion; } set { this.RaiseAndSetIfChanged(a => a.PlatformVersion, ref Rec.PlatformVersion, value); } }

        /// <summary>接続フレームワーク名</summary>
        public string SoftwareName { get { return Rec.SoftwareName; } set { this.RaiseAndSetIfChanged(a => a.SoftwareName, ref Rec.SoftwareName, value); } }

        /// <summary>接続フレームワーク名</summary>
        public string SoftwareVersion { get { return Rec.SoftwareVersion; } set { this.RaiseAndSetIfChanged(a => a.SoftwareVersion, ref Rec.SoftwareVersion, value); } }

        #endregion
        #region プロパティ(Growl互換[INotification])

        /// <summary>アプリケーション名</summary>
        public string ApplicationName { get { return Rec.ApplicationName; } set { this.RaiseAndSetIfChanged(a => a.ApplicationName, ref Rec.ApplicationName, value); } }

        /// <summary>通知ID</summary>
        public string ID { get { return Rec.ID; } set { this.RaiseAndSetIfChanged(a => a.ID, ref Rec.ID, value); } }

        /// <summary>更新ID。値が同じ旧通知を置き換えます。</summary>
        public string CoalescingID { get { return Rec.CoalescingID; } set { this.RaiseAndSetIfChanged(a => a.CoalescingID, ref Rec.CoalescingID, value); } }

        /// <summary>優先度</summary>
        public NeithLogPriority Priority { get { return Rec.Priority; } set { this.RaiseAndSetIfChanged(a => a.Priority, ref Rec.Priority, value); } }

        /// <summary>確認待ちフラグ。trueの場合、ユーザ確認が行われるまで表示し続けます。</summary>
        public bool Sticky { get { return Rec.Sticky; } set { this.RaiseAndSetIfChanged(a => a.Sticky, ref Rec.Sticky, value); } }

        /// <summary>タイトル</summary>
        public string Title { get { return Rec.Title; } set { this.RaiseAndSetIfChanged(a => a.Title, ref Rec.Title, value); } }

        /// <summary>本文</summary>
        public string Text { get { return Rec.Text; } set { this.RaiseAndSetIfChanged(a => a.Text, ref Rec.Text, value); } }


        #endregion
        #region プロパティ(Signpost保存用)

        /// <summary>受付時刻(UTC時刻、IDを兼ねるためUnique保証)</summary>
        public DateTime ReceptionTime { get { return Rec.ReceptionTime; } set { this.RaiseAndSetIfChanged(a => a.ReceptionTime, ref Rec.ReceptionTime, value); } }


        #endregion
        #region プロパティ(Signpost拡張)

        /// <summary>イベント発生時刻</summary>
        public DateTimeOffset EventTime { get { return Rec.EventTime; } set { this.RaiseAndSetIfChanged(a => a.EventTime, ref Rec.EventTime, value); } }

        /// <summary>プロセスID</summary>
        public int Pid { get { return Rec.Pid; } set { this.RaiseAndSetIfChanged(a => a.Pid, ref Rec.Pid, value); } }

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain { get { return Rec.Domain; } set { this.RaiseAndSetIfChanged(a => a.Domain, ref Rec.Domain, value); } }

        /// <summary>ログの取得元ユーザ</summary>
        public string User { get { return Rec.User; } set { this.RaiseAndSetIfChanged(a => a.User, ref Rec.User, value); } }

        /// <summary>ログ内容</summary>
        public string LogText { get { return Rec.LogText; } set { this.RaiseAndSetIfChanged(a => a.LogText, ref Rec.LogText, value); } }

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd { get { return Rec.HWnd; } set { this.RaiseAndSetIfChanged(a => a.HWnd, ref Rec.HWnd, value); } }

        /// <summary>ログのカテゴリ</summary>
        public string Category { get { return Rec.Category; } set { this.RaiseAndSetIfChanged(a => a.Category, ref Rec.Category, value); } }

        /// <summary>ログのタイプ</summary>
        public string Type { get { return Rec.Type; } set { this.RaiseAndSetIfChanged(a => a.Type, ref Rec.Type, value); } }

        /// <summary>行為の実行者</summary>
        public string Actor { get { return Rec.Actor; } set { this.RaiseAndSetIfChanged(a => a.Actor, ref Rec.Actor, value); } }

        /// <summary>行為の対象</summary>
        public string Target { get { return Rec.Target; } set { this.RaiseAndSetIfChanged(a => a.Target, ref Rec.Target, value); } }

        /// <summary>アイコン画像URL</summary>
        public string Icon { get { return Rec.Icon; } set { this.RaiseAndSetIfChanged(a => a.Icon, ref Rec.Icon, value); } }

        #endregion
        #region メソッド
        public static NeithLogModel Create()
        {
            return new NeithLogModel(NeithLog.Create());
        }

        public override string ToString()
        {
            return Rec.ToString();
        }

        #endregion

        public bool Equals(NeithLogModel other)
        {
            if (other == null) return false;
            return Rec.Equals(other.Rec);
        }

    }
}
