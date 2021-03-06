using System;
using System.ComponentModel;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// 通知優先度。
    /// </summary>
    /// <remarks>
    /// The priority of a notification can be used to change the way the notification is handled
    /// and presented to the end user. For instance, higher priority notifications might be displayed
    /// with a red color or exclamation icon. However, each display is responsible for
    /// handling changes related to priority and may not make any distinction between different priority
    /// levels. Further, although each notification can request its own priority, the end user may elect 
    /// to override this priority setting, so the notification's requested priority is not guaranteed.
    /// </remarks>
    [Description("通知優先度")]
    public enum Priority
    {
        /// <summary>ゆっくり</summary>
        [Description("ゆっくり")]
        VeryLow = -2,

        /// <summary>控えめ</summary>
        [Description("控えめ")]
        Moderate = -1,

        /// <summary>通常</summary>
        [Description("通常")]
        Normal = 0,

        /// <summary>優先</summary>
        [Description("優先")]
        High = 1,

        /// <summary>緊急</summary>
        [Description("緊急")]
        Emergency = 2
    }
}
