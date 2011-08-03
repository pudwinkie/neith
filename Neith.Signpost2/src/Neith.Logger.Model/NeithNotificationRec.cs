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
    public class NeithNotificationRec : IEquatable<NeithNotificationRec>
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

        /// <summary>プロセスID</summary>
        public int Pid;

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain;

        /// <summary>ログの取得元ユーザ</summary>
        public string User;

        /// <summary>ログ内容</summary>
        public string LogText;

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd;

        /// <summary>ログのカテゴリ</summary>
        public string Category;

        /// <summary>ログのタイプ</summary>
        public string Type;

        /// <summary>行為の対象</summary>
        public string Target;

        #endregion
        #region メソッド
        public static NeithNotificationRec Create()
        {
            var obj = new NeithNotificationRec();
            obj.ReceptionTime = UniqueTime.UtcNow;
            return obj;
        }

        public override string ToString()
        {
            return string.Format("{0:O}: {1}", ReceptionTime.ToLocalTime(), Text);
        }

        #endregion

        public bool Equals(NeithNotificationRec other)
        {
            return this.ReceptionTime == other.ReceptionTime
                && this.MachineName == other.MachineName
                && this.Pid == other.Pid
                && this.SoftwareName == other.SoftwareName
                && this.Domain == other.Domain
                && this.User == other.User
                && this.LogText == other.LogText
                ;
        }
    }
}