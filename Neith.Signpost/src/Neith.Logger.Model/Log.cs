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
        public DateTimeOffset Timestamp
        {
            get
            {
                if (_timestamp == DateTimeOffset.MinValue) {
                    _timestamp = new DateTimeOffset(
                    DateTime.FromBinary(TimestampBinary),
                    TimeSpan.FromTicks(TimestampOffset));
                }
                return _timestamp;
            }
            set
            {
                _timestamp = value;
                TimestampBinary = value.DateTime.ToBinary();
                TimestampOffset = value.Offset.Ticks;
            }
        }
        private DateTimeOffset _timestamp = DateTimeOffset.MinValue;

        [ProtoMember(1)]
        private long TimestampBinary { get; set; }
        [ProtoMember(2)]
        private long TimestampOffset { get; set; }


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


        /// <summary>ログのカテゴリ</summary>
        [ProtoMember(9)]
        public string Category { get; set; }


        /// <summary>ログのタイプ</summary>
        [ProtoMember(10)]
        public string Type { get; set; }


        /// <summary>ログの優先度</summary>
        public TraceEventType Priority { get; set; }
        [ProtoMember(11)]
        private int _priority
        {
            get { return (int)Priority; }
            set { Priority = (TraceEventType)value; }
        }


        /// <summary>ログのメッセージ</summary>
        [ProtoMember(12)]
        public string Message { get; set; }



        /// <summary>ログの分析モジュール</summary>
        [ProtoMember(13)]
        public string Analyzer { get; set; }





    }
}
