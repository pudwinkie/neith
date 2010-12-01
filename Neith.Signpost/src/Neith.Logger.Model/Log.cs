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
                return new DateTimeOffset(
                    DateTime.FromBinary(TimestampBinary),
                    TimeSpan.FromTicks(TimestampOffset));
            }
            set
            {
                TimestampBinary = value.DateTime.ToBinary();
                TimestampOffset = value.Offset.Ticks;
            }
        }
        [ProtoMember(1, IsRequired = true)]
        private long TimestampBinary { get; set; }
        [ProtoMember(2, IsRequired = true)]
        private long TimestampOffset { get; set; }

        /// <summary>ログの収集モジュール</summary>
        [ProtoMember(3, IsRequired = true)]
        public string Collector { get; set; }

        /// <summary>ホスト</summary>
        [ProtoMember(4, IsRequired = true)]
        public string Host { get; set; }

        /// <summary>プロセスID</summary>
        [ProtoMember(5, IsRequired = true)]
        public int Pid { get; set; }

        /// <summary>ログの取得元アプリ</summary>
        [ProtoMember(6, IsRequired = true)]
        public string Application { get; set; }

        /// <summary>ログの取得元ドメイン</summary>
        [ProtoMember(7, IsRequired = true)]
        public string Domain { get; set; }

        /// <summary>ログの取得元ドメイン</summary>
        [ProtoMember(8, IsRequired = true)]
        public string User { get; set; }

        /// <summary>ログの分析モジュール</summary>
        [ProtoMember(9, IsRequired = true)]
        public string Analyzer { get; set; }

        /// <summary>ログのカテゴリ</summary>
        [ProtoMember(10, IsRequired = true)]
        public string Category { get; set; }

        /// <summary>ログのタイプ</summary>
        [ProtoMember(11, IsRequired = true)]
        public string Type { get; set; }

        /// <summary>ログの優先度</summary>
        public TraceEventType Priority { get; set; }
        [ProtoMember(12, IsRequired = true)]
        private int _priority
        {
            get { return (int)Priority; }
            set { Priority = (TraceEventType)value; }
        }


    }
}
