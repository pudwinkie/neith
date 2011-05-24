using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ProtoBuf;

namespace Neith.Logger.Model
{
    [ProtoContract]
    public class NeithLog : IDictionary<string, string>, IEquatable<NeithLog>
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


        /// <summary>ログ内容(文字列のDIC)</summary>
        public IDictionary<string, string> Items { get; private set; }
        [ProtoMember(9)]
        private KeyValuePair<string, string>[] ItemData
        {
            get { return Items.ToArray(); }
            set
            {
                Items = new Dictionary<string, string>();
                foreach (var pair in value) Items.Add(pair);
            }
        }

        /// <summary>ログの分析モジュール</summary>
        [ProtoMember(10)]
        public string Analyzer { get; set; }


        #endregion
        #region 解析プロパティ（保存対象外）

        /// <summary>WindowHandle</summary>
        public IntPtr HWnd { get; set; }

        /// <summary>ログのカテゴリ</summary>
        public string Category { get; set; }


        /// <summary>ログのタイプ</summary>
        public string Type { get; set; }


        /// <summary>ログの優先度</summary>
        public NeithLogPriority Priority { get; set; }


        /// <summary>行為の実行者</summary>
        public string Actor { get; set; }


        /// <summary>行為の対象</summary>
        public string Target { get; set; }


        /// <summary>解析結果メッセージ</summary>
        public string Message { get; set; }


        /// <summary>アイコン画像URL</summary>
        public string Icon { get; set; }


        #endregion
        #region メソッド
        public static NeithLog Create()
        {
            var log = new NeithLog();
            log.Timestamp = DateTimeOffset.Now;
            log.Id = Guid.NewGuid();
            return log;
        }

        private NeithLog()
        {
            if (Items == null) Items = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return string.Format("{0:O}: {1}", Timestamp, Message);
        }

        #endregion

        public bool Equals(NeithLog other)
        {
            return this.Timestamp == other.Timestamp
                && this.Id == other.Id
                && this.Collector == other.Collector
                && this.Host == other.Host
                && this.Pid == other.Pid
                && this.Application == other.Application
                && this.Domain == other.Domain
                && this.User == other.User
                && this.Analyzer == other.Analyzer
                && this.Items.SequenceEqual(other.Items)
                ;
        }

        public ICollection<string> Keys { get { return Items.Keys; } }
        public ICollection<string> Values { get { return Items.Values; } }
        public int Count { get { return Items.Count; } }
        public bool IsReadOnly { get { return Items.IsReadOnly; } }

        public string this[string key]
        {
            get { return Items[key]; }
            set { Items[key] = value; }
        }

        public void Clear() { Items.Clear(); }
        public bool ContainsKey(string key) { return Items.ContainsKey(key); }
        public bool Remove(string key) { return Items.Remove(key); }
        public void Add(string key, string value) { Items.Add(key, value); }
        public bool TryGetValue(string key, out string value) { return Items.TryGetValue(key, out value); }

        public void Add(KeyValuePair<string, string> item) { Items.Add(item); }
        public bool Contains(KeyValuePair<string, string> item) { return Items.Contains(item); }
        public bool Remove(KeyValuePair<string, string> item) { return Items.Remove(item); }
        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex) { Items.CopyTo(array, arrayIndex); }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator() { return Items.GetEnumerator(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return Items.GetEnumerator(); }

    }
}