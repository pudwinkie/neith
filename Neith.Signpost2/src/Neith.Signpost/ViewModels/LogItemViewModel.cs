using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using Neith.Logger.Model;

namespace Neith.Signpost
{
	public class LogItemViewModel : INotifyPropertyChanged
	{
        private readonly NeithLog item;


		public LogItemViewModel()
		{
            var demo = NeithLog.Create();
            demo.Actor = "Demo Data";
            demo.Message = "デバッグ表示のメッセージ";
            item = demo;
		}

        public LogItemViewModel(NeithLog item)
        {
            this.item = item;
        }


        #region プロパティ
        /// <summary>タイムスタンプ</summary>
        public DateTimeOffset Timestamp { get { return item.UtcTime; } }

        /// <summary>タイムスタンプ(UTC時刻)</summary>
        public DateTime UtcTime { get { return item.UtcTime; } }

        /// <summary>タイムスタンプ(現地時刻)</summary>
        public DateTime LocalTime { get { return item.UtcTime.ToLocalTime(); } }

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
        public string LogText { get { return item.LogText; } }

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
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion
	}
}