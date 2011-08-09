using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;
using Wintellect.Sterling;

namespace Neith.Logger.Model
{
    [Serializable, DataContract]
    public class NeithNotificationRec
    {
        #region フィールド(Signpost KEY)
        /// <summary>受付時刻(UTC時刻、IDを兼ねるためUnique保証)</summary>
        [Key]
        public DateTime ReceptionTime;

        #endregion
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

        /// <summary>カスタムバイナリ属性</summary>
        public Dictionary<string, Resource> CustomBinaryAttributes;

        /// <summary>カスタム文字列属性</summary>
        public Dictionary<string, string> CustomTextAttributes;


        #endregion
        #region フィールド(Growl互換[IIcon])
        /// <summary>アイコン</summary>
        public Resource Icon;

        /// <summary>行為の実行者</summary>
        public string Name;


        #endregion
        #region フィールド(Growl互換[INotification])

        /// <summary>アプリケーション名</summary>
        public string ApplicationName;

        /// <summary>通知ID</summary>
        public string ID;

        /// <summary>更新ID。値が同じ旧通知を置き換えます。</summary>
        public string CoalescingID;

        /// <summary>優先度</summary>
        public Priority Priority;

        /// <summary>確認待ちフラグ。trueの場合、ユーザ確認が行われるまで表示し続けます。</summary>
        public bool Sticky;

        /// <summary>タイトル</summary>
        public string Title;

        /// <summary>本文</summary>
        public string Text;


        #endregion
        #region フィールド(Signpost拡張)

        /// <summary>イベント発生時刻</summary>
        public DateTimeOffset EventTime;

        /// <summary>プロセスへのヒモ付情報</summary>
        public string ProcessKey;

        /// <summary>ログの取得元ユーザ</summary>
        public string User;

        /// <summary>ログ内容</summary>
        public string LogText;

        /// <summary>ログのカテゴリ</summary>
        public string Category;

        /// <summary>ログのタイプ</summary>
        public string Type;

        /// <summary>行為の対象</summary>
        public string Target;

        #endregion
        #region オブジェクト作成
        public static NeithNotificationRec Create()
        {
            var obj = new NeithNotificationRec();
            obj.ReceptionTime = UniqueTime.UtcNow;
            return obj;
        }

        /// <summary>
        /// ヘッダオブジェクトよりレコードを作成します。
        /// </summary>
        public static NeithNotificationRec FromHeaders(HeaderCollection headers)
        {
            var p = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_PRIORITY, false);
            var priority = Neith.Growl.Connector.Priority.Normal;
            if (p != null) {
                int pval = 0;
                bool pok = int.TryParse(p, out pval);
                if (pok && Enum.IsDefined(typeof(Priority), pval))
                    priority = (Priority)pval;
            }
            var text = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_TEXT, false);
            if (text == null) text = String.Empty;

            var item = new NeithNotificationRec()
            {
                Name = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_NAME, true),
                Icon = headers.GetHeaderResourceValue(HeaderKeys.NOTIFICATION_ICON, false),

                ApplicationName = headers.GetHeaderStringValue(HeaderKeys.APPLICATION_NAME, true),
                ID = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_ID, false),
                CoalescingID = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_COALESCING_ID, false),
                Priority = priority,
                Sticky = headers.GetHeaderBooleanValue(HeaderKeys.NOTIFICATION_STICKY, false),
                Title = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_TITLE, true),
                Text = text,

                EventTime = headers.GetHeaderDateTimeOffsetValue(NeithHeaderKeys.EVENT_TIME, false),
                ProcessKey = headers.GetHeaderStringValue(NeithHeaderKeys.PROCESS_KEY, false),
                User = headers.GetHeaderStringValue(NeithHeaderKeys.USER, false),
                LogText = headers.GetHeaderStringValue(NeithHeaderKeys.LOGTEXT, false),
                Category = headers.GetHeaderStringValue(NeithHeaderKeys.CATEGORY, false),
                Type = headers.GetHeaderStringValue(NeithHeaderKeys.TYPE, false),
                Target = headers.GetHeaderStringValue(NeithHeaderKeys.TARGET, false),
            };
            item.ToExtensibleObject().SetInhertiedAttributesFromHeaders(headers);
            return item;
        }

        #endregion
        #region 変換

        public IExtensibleObject ToExtensibleObject()
        {
            return new MyExtensibleObject(this);
        }

        /// <summary>基本情報</summary>
        private class MyExtensibleObject : IExtensibleObject
        {
            private readonly NeithNotificationRec Rec;
            public MyExtensibleObject(NeithNotificationRec rec)
            {
                Rec = rec;
            }

            /// <summary>カスタムバイナリ属性</summary>
            public Dictionary<string, Resource> CustomBinaryAttributes { get { return Rec.CustomBinaryAttributes; } }

            /// <summary>カスタム文字列属性</summary>
            public Dictionary<string, string> CustomTextAttributes { get { return Rec.CustomTextAttributes; } }

            /// <summary>マシン名</summary>
            public string MachineName { get { return Rec.MachineName; } set { Rec.MachineName = value; } }

            /// <summary>プラットフォーム(OS)名</summary>
            public string PlatformName { get { return Rec.PlatformName; } set { Rec.PlatformName = value; } }

            /// <summary>プラットフォーム(OS)バージョン</summary>
            public string PlatformVersion { get { return Rec.PlatformVersion; } set { Rec.PlatformVersion = value; } }

            /// <summary>ソフトウェア名</summary>
            public string SoftwareName { get { return Rec.SoftwareName; } set { Rec.SoftwareName = value; } }

            /// <summary>ソフトウェアバージョン</summary>
            public string SoftwareVersion { get { return Rec.SoftwareVersion; } set { Rec.SoftwareVersion = value; } }
        }


        #endregion
        #region 一般メソッド

        public override string ToString()
        {
            return string.Format("{0:O}: {1}", ReceptionTime.ToLocalTime(), Text);
        }

        #endregion

    }

}