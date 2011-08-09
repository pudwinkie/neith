using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;
using ReactiveUI;

namespace Neith.Logger.Model
{
    /// <summary>
    /// ログモデル。
    /// </summary>
    [Serializable, DataContract]
    public class NeithNotificationModel : ReactiveValidatedObject, INeithNotification
    {
        /// <summary>データ本体</summary>
        public NeithNotificationRec Rec { get; private set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="rec"></param>
        public NeithNotificationModel(NeithNotificationRec rec)
        {
            Rec = rec;
        }

        #region プロパティ(Signpost保存Key)

        /// <summary>受付時刻(UTC時刻、IDを兼ねるためUnique保証)</summary>
        [IgnoreDataMember]
        public DateTime ReceptionTime { get { return Rec.ReceptionTime; } set { this.RaiseAndSetIfChanged(a => a.ReceptionTime, ref Rec.ReceptionTime, value); } }


        #endregion
        #region プロパティ(Growl互換[IExtensibleObject])
        /// <summary>マシン名</summary>
        [IgnoreDataMember]
        public string MachineName { get { return Rec.MachineName; } set { this.RaiseAndSetIfChanged(a => a.MachineName, ref Rec.MachineName, value); } }

        /// <summary>プラットフォーム(OS)名</summary>
        [IgnoreDataMember]
        public string PlatformName { get { return Rec.PlatformName; } set { this.RaiseAndSetIfChanged(a => a.PlatformName, ref Rec.PlatformName, value); } }

        /// <summary>プラットフォーム(OS)バージョン</summary>
        [IgnoreDataMember]
        public string PlatformVersion { get { return Rec.PlatformVersion; } set { this.RaiseAndSetIfChanged(a => a.PlatformVersion, ref Rec.PlatformVersion, value); } }

        /// <summary>接続フレームワーク名</summary>
        [IgnoreDataMember]
        public string SoftwareName { get { return Rec.SoftwareName; } set { this.RaiseAndSetIfChanged(a => a.SoftwareName, ref Rec.SoftwareName, value); } }

        /// <summary>接続フレームワーク名</summary>
        [IgnoreDataMember]
        public string SoftwareVersion { get { return Rec.SoftwareVersion; } set { this.RaiseAndSetIfChanged(a => a.SoftwareVersion, ref Rec.SoftwareVersion, value); } }

        /// <summary>カスタムバイナリ属性</summary>
        [IgnoreDataMember]
        public Dictionary<string, Resource> CustomBinaryAttributes { get { return Rec.CustomBinaryAttributes; } }

        /// <summary>カスタム文字列属性</summary>
        [IgnoreDataMember]
        public Dictionary<string, string> CustomTextAttributes { get { return Rec.CustomTextAttributes; } }


        #endregion
        #region プロパティ(Growl互換[IIcon])
        /// <summary>アイコン</summary>
        [IgnoreDataMember]
        public Resource Icon { get { return Rec.Icon; } set { this.RaiseAndSetIfChanged(a => a.Icon, ref Rec.Icon, value); } }

        /// <summary>行為の実行者</summary>
        [IgnoreDataMember]
        public string Name { get { return Rec.Name; } set { this.RaiseAndSetIfChanged(a => a.Name, ref Rec.Name, value); } }


        #endregion
        #region プロパティ(Growl互換[INotification])

        /// <summary>通知元アプリケーション</summary>
        [IgnoreDataMember]
        public string ApplicationName { get { return Rec.ApplicationName; } set { this.RaiseAndSetIfChanged(a => a.ApplicationName, ref Rec.ApplicationName, value); } }

        /// <summary>通知グループID。旧通知を置き換える場合、同じグループIDで通知します。</summary>
        [IgnoreDataMember]
        public string CoalescingID { get { return Rec.CoalescingID; } set { this.RaiseAndSetIfChanged(a => a.CoalescingID, ref Rec.CoalescingID, value); } }

        /// <summary>通知ID</summary>
        [IgnoreDataMember]
        public string ID { get { return Rec.ID; } set { this.RaiseAndSetIfChanged(a => a.ID, ref Rec.ID, value); } }

        /// <summary>優先度</summary>
        [IgnoreDataMember]
        public Priority Priority { get { return Rec.Priority; } set { this.RaiseAndSetIfChanged(a => a.Priority, ref Rec.Priority, value); } }

        /// <summary>確認待ちフラグ。trueの場合、ユーザ確認が行われるまで表示し続けます。</summary>
        [IgnoreDataMember]
        public bool Sticky { get { return Rec.Sticky; } set { this.RaiseAndSetIfChanged(a => a.Sticky, ref Rec.Sticky, value); } }

        /// <summary>タイトル</summary>
        [IgnoreDataMember]
        public string Title { get { return Rec.Title; } set { this.RaiseAndSetIfChanged(a => a.Title, ref Rec.Title, value); } }

        /// <summary>本文</summary>
        [IgnoreDataMember]
        public string Text { get { return Rec.Text; } set { this.RaiseAndSetIfChanged(a => a.Text, ref Rec.Text, value); } }


        #endregion
        #region プロパティ(Signpost拡張)

        /// <summary>イベント発生時刻</summary>
        [IgnoreDataMember]
        public DateTimeOffset EventTime { get { return Rec.EventTime; } set { this.RaiseAndSetIfChanged(a => a.EventTime, ref Rec.EventTime, value); } }

        /// <summary>プロセスへのヒモ付情報</summary>
        [IgnoreDataMember]
        public string ProcessKey { get { return Rec.ProcessKey; } set { this.RaiseAndSetIfChanged(a => a.ProcessKey, ref Rec.ProcessKey, value); } }

        /// <summary>ログの取得元ユーザ</summary>
        [IgnoreDataMember]
        public string User { get { return Rec.User; } set { this.RaiseAndSetIfChanged(a => a.User, ref Rec.User, value); } }

        /// <summary>ログ内容</summary>
        [IgnoreDataMember]
        public string LogText { get { return Rec.LogText; } set { this.RaiseAndSetIfChanged(a => a.LogText, ref Rec.LogText, value); } }

        /// <summary>ログのカテゴリ</summary>
        [IgnoreDataMember]
        public string Category { get { return Rec.Category; } set { this.RaiseAndSetIfChanged(a => a.Category, ref Rec.Category, value); } }

        /// <summary>ログのタイプ</summary>
        [IgnoreDataMember]
        public string Type { get { return Rec.Type; } set { this.RaiseAndSetIfChanged(a => a.Type, ref Rec.Type, value); } }

        /// <summary>行為の対象</summary>
        [IgnoreDataMember]
        public string Target { get { return Rec.Target; } set { this.RaiseAndSetIfChanged(a => a.Target, ref Rec.Target, value); } }


        #endregion
        #region オブジェクト作成

        public static NeithNotificationModel Create()
        {
            return new NeithNotificationModel(NeithNotificationRec.Create());
        }

        /// <summary>
        /// ヘッダオブジェクトよりレコードを作成します。
        /// </summary>
        public static NeithNotificationModel FromHeaders(HeaderCollection headers)
        {
            var rec = NeithNotificationRec.FromHeaders(headers);
            return new NeithNotificationModel(rec);
        }

        #endregion
        #region メソッド

        public override string ToString()
        {
            return Rec.ToString();
        }

        #endregion

    }
}
