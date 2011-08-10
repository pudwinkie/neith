using System;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>通知</summary>
    public interface INotification : IExtensibleObject, IIcon
    {
        /// <summary>通知元アプリケーション</summary>
        string ApplicationName { get; set; }

        /// <summary>通知ID</summary>
        string ID { get; set; }

        /// <summary>更新ID。値が同じ旧通知を置き換えます。</summary>
        string CoalescingID { get; set; }

        /// <summary>優先度</summary>
        Priority Priority { get; set; }

        /// <summary>確認待ちフラグ</summary>
        bool Sticky { get; set; }

        /// <summary>タイトル</summary>
        string Title { get; set; }

        /// <summary>本文</summary>
        string Text { get; set; }

    }
}
