using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ProtoBuf;

namespace Neith.Logger.Model
{
    [ProtoContract]
    public class Log : IEquatable<Log>
    {
        #region ログ保存プロパティ
        /// <summary>タイムスタンプ</summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>タイムスタンプ(UTC時刻)</summary>
        public DateTime TimestampUTC { get { return Timestamp.UtcDateTime; } }

        [ProtoMember(1)]
        private long TimestampBinary
        {
            get { return TimestampUTC.ToBinary(); }
            set { Timestamp = new DateTimeOffset(DateTime.FromBinary(value).ToLocalTime()); }
        }

        /// <summary>ログのGUID</summary>
        [ProtoMember(2)]
        public Guid Id { get; set; }


        /// <summary>ログの収集モジュール</summary>
        [ProtoMember(3)]
        public string Collector { get; set; }


        /// <summary>ホスト</summary>
        [ProtoMember(4)]
        public string Host { get; set; }


        /// <summary>プロセスID</summary>
        [ProtoMember(5)]
        public int Pid { get; set; }


        /// <summary>ログの取得元アプリ</summary>
        [ProtoMember(6)]
        public string Application { get; set; }


        /// <summary>ログの取得元ドメイン</summary>
        [ProtoMember(7)]
        public string Domain { get; set; }


        /// <summary>ログの取得元ユーザ</summary>
        [ProtoMember(8)]
        public string User { get; set; }


        /// <summary>ログのメッセージ</summary>
        [ProtoMember(9)]
        public string Message { get; set; }


        /// <summary>ログの分析モジュール</summary>
        [ProtoMember(10)]
        public string Analyzer { get; set; }


        #endregion
        #region 解析プロパティ（保存対象外）

        /// <summary>ログのカテゴリ</summary>
        public string Category { get; set; }


        /// <summary>ログのタイプ</summary>
        public string Type { get; set; }


        /// <summary>ログの優先度</summary>
        public LogPriority Priority { get; set; }


        /// <summary>行為の実行者</summary>
        public string Actor { get; set; }


        /// <summary>行為の対象</summary>
        public string Target { get; set; }


        /// <summary>アイコン画像URL</summary>
        public string Icon { get; set; }


        #endregion
        #region メソッド
        public static Log Create()
        {
            var log = new Log();
            log.Timestamp = DateTimeOffset.Now;
            log.Id = Guid.NewGuid();
            return log;
        }

        private Log() { }

        public bool Equals(Log other)
        {
            if (other.IsNull()) return false;
            return this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Log);
        }


        public static bool operator ==(Log a, Log b)
        {
            if (a.IsNull()) return false;
            return a.Equals(b);
        }

        public static bool operator !=(Log a, Log b) { return !(a == b); }

        #endregion

    }
}