using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ProtoBuf;

namespace Neith.Logger.Model
{
    [ProtoContract]
    public class Log
    {
        /// <summary>タイムスタンプ</summary>
        public DateTimeOffset Timestamp{get;set;}

        /// <summary>タイムスタンプ(UTC時刻)</summary>
        public DateTime TimestampUTC { get { return Timestamp.UtcDateTime; } }

        [ProtoMember(1)]
        private long TimestampBinary
        {
            get { return TimestampUTC.ToBinary(); }
            set { Timestamp = new DateTimeOffset(DateTime.FromBinary(value).ToLocalTime()); }
        }


        /// <summary>ログの収集モジュール</summary>
        [ProtoMember(2)]
        public string Collector { get; set; }


        /// <summary>ホスト</summary>
        [ProtoMember(3)]
        public string Host { get; set; }


        /// <summary>プロセスID</summary>
        [ProtoMember(4)]
        public int Pid { get; set; }


        /// <summary>ログの取得元アプリ</summary>
        [ProtoMember(5)]
        public string Application { get; set; }


        /// <summary>ログの取得元ドメイン</summary>
        [ProtoMember(6)]
        public string Domain { get; set; }


        /// <summary>ログの取得元ユーザ</summary>
        [ProtoMember(7)]
        public string User { get; set; }


        /// <summary>ログのカテゴリ</summary>
        [ProtoMember(8)]
        public string Category { get; set; }


        /// <summary>ログのタイプ</summary>
        [ProtoMember(9)]
        public string Type { get; set; }


        /// <summary>ログの優先度</summary>
        [ProtoMember(10)]
        public LogPriority Priority { get; set; }


        /// <summary>行為を行った者</summary>
        [ProtoMember(11)]
        public string Actor { get; set; }


        /// <summary>行為の対象者</summary>
        [ProtoMember(12)]
        public string Target { get; set; }


        /// <summary>ログのメッセージ</summary>
        [ProtoMember(13)]
        public string Message { get; set; }


        /// <summary>ログの分析モジュール</summary>
        [ProtoMember(14)]
        public string Analyzer { get; set; }


        /// <summary>アイコン画像URL</summary>
        [ProtoMember(15)]
        public string Icon { get; set; }


        // 14,15は現在のところ予約。かならず使うようなものがあれば追加する。




    }
}
