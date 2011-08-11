using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Logger.Model
{
    public class NeithNotificationRecWrapper:INeithNotification
    {
        /// <summary>データ本体</summary>
        public NeithNotificationRec Rec { get; private set; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="rec"></param>
        public NeithNotificationRecWrapper(NeithNotificationRec rec)
        {
            Rec = rec;
        }

        #region プロパティ(Signpost保存Key)

        /// <summary>受付時刻(UTC時刻、IDを兼ねるためUnique保証)</summary>
        public DateTime ReceptionTime { get { return Rec.ReceptionTime; } set { Rec.ReceptionTime = value; } }


        #endregion
        #region プロパティ(Growl互換[IExtensibleObject])
        /// <summary>マシン名</summary>
        public string MachineName { get { return Rec.MachineName; } set { Rec.MachineName = value; } }

        /// <summary>プラットフォーム(OS)名</summary>
        public string PlatformName { get { return Rec.PlatformName; } set { Rec.PlatformName = value; } }

        /// <summary>プラットフォーム(OS)バージョン</summary>
        public string PlatformVersion { get { return Rec.PlatformVersion; } set { Rec.PlatformVersion = value; } }

        /// <summary>接続フレームワーク名</summary>
        public string SoftwareName { get { return Rec.SoftwareName; } set { Rec.SoftwareName = value; } }

        /// <summary>接続フレームワーク名</summary>
        public string SoftwareVersion { get { return Rec.SoftwareVersion; } set { Rec.SoftwareVersion = value; } }

        /// <summary>カスタムバイナリ属性</summary>
        public Dictionary<string, Resource> CustomBinaryAttributes { get { return Rec.CustomBinaryAttributes; } }

        /// <summary>カスタム文字列属性</summary>
        public Dictionary<string, string> CustomTextAttributes { get { return Rec.CustomTextAttributes; } }


        #endregion
        #region プロパティ(Growl互換[IIcon])
        /// <summary>アイコン</summary>
        public Resource Icon { get { return Rec.Icon; } set { Rec.Icon = value; } }

        /// <summary>行為の実行者</summary>
        public string Name { get { return Rec.Name; } set { Rec.Name = value; } }


        #endregion
        #region プロパティ(Growl互換[INotification])

        /// <summary>通知元アプリケーション</summary>
        public string ApplicationName { get { return Rec.ApplicationName; } set { Rec.ApplicationName = value; } }

        /// <summary>通知グループID。旧通知を置き換える場合、同じグループIDで通知します。</summary>
        public string CoalescingID { get { return Rec.CoalescingID; } set { Rec.CoalescingID = value; } }

        /// <summary>通知ID</summary>
        public string ID { get { return Rec.ID; } set { Rec.ID = value; } }

        /// <summary>優先度</summary>
        public Priority Priority { get { return Rec.Priority; } set { Rec.Priority = value; } }

        /// <summary>確認待ちフラグ。trueの場合、ユーザ確認が行われるまで表示し続けます。</summary>
        public bool Sticky { get { return Rec.Sticky; } set { Rec.Sticky = value; } }

        /// <summary>タイトル</summary>
        public string Title { get { return Rec.Title; } set { Rec.Title = value; } }

        /// <summary>本文</summary>
        public string Text { get { return Rec.Text; } set { Rec.Text = value; } }


        #endregion
        #region プロパティ(Signpost拡張)

        /// <summary>イベント発生時刻</summary>
        public DateTimeOffset EventTime { get { return Rec.EventTime; } set { Rec.EventTime = value; } }

        /// <summary>プロセスへのヒモ付情報</summary>
        public string ProcessKey { get { return Rec.ProcessKey; } set { Rec.ProcessKey = value; } }

        /// <summary>ログの取得元ユーザ</summary>
        public string User { get { return Rec.User; } set { Rec.User = value; } }

        /// <summary>ログ内容</summary>
        public string LogText { get { return Rec.LogText; } set { Rec.LogText = value; } }

        /// <summary>ログのカテゴリ</summary>
        public string Category { get { return Rec.Category; } set { Rec.Category = value; } }

        /// <summary>ログのタイプ</summary>
        public string Type { get { return Rec.Type; } set { Rec.Type = value; } }

        /// <summary>行為の対象</summary>
        public string Target { get { return Rec.Target; } set { Rec.Target = value; } }


        #endregion
    }
}
