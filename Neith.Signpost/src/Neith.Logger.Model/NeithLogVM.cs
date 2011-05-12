using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Neith.Logger.Model
{
    public class NeithLogVM : INotifyPropertyChanged
    {
        private readonly NeithLog item;

        public NeithLogVM(NeithLog item)
        {
            this.item = item;
        }

        #region プロパティ
        /// <summary>タイムスタンプ</summary>
        public DateTimeOffset Timestamp { get { return item.Timestamp; } }

        /// <summary>タイムスタンプ(UTC時刻)</summary>
        public DateTime TimestampUTC { get { return item.TimestampUTC; } }

        /// <summary>タイムスタンプ(現地時刻)</summary>
        public DateTime TimestampLocal { get { return item.Timestamp.ToLocalTime().DateTime; } }

        /// <summary>ログのGUID</summary>
        public Guid Id { get { return item.Id; } }

        /// <summary>ログの収集モジュール</summary>
        public string Collector { get { return item.Collector; } }

        /// <summary>ホスト</summary>
        public string Host { get { return item.Host; } }

        /// <summary>プロセスID</summary>
        public int Pid { get { return item.Pid; } }

        /// <summary>ログの取得元アプリ</summary>
        public string Application { get { return item.Application; } }

        /// <summary>ログの取得元ドメイン</summary>
        public string Domain { get { return item.Domain; } }

        /// <summary>ログの取得元ユーザ</summary>
        public string User { get { return item.User; } }

        /// <summary>ログ内容(文字列のDIC)</summary>
        public IDictionary<string, string> Items { get { return item.Items; } }

        /// <summary>ログの分析モジュール</summary>
        public string Analyzer { get { return item.Analyzer; } }



        /// <summary>ログのカテゴリ</summary>
        public string Category { get { return item.Category; } }

        /// <summary>ログのタイプ</summary>
        public string Type { get { return item.Type; } }

        /// <summary>ログの優先度</summary>
        public NeithLogPriority Priority { get { return item.Priority; } }

        /// <summary>行為の実行者</summary>
        public string Actor { get { return item.Actor; } }

        /// <summary>行為の対象</summary>
        public string Target { get { return item.Target; } }

        /// <summary>解析結果メッセージ</summary>
        public string Message { get { return item.Message; } }

        /// <summary>アイコン画像URL</summary>
        public string Icon { get { return item.Icon; } }


        #endregion
        #region INotifyPropertyChanged メンバー

        /// <summary>プロパティ変更イベント</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged.IsNull()) return;
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}