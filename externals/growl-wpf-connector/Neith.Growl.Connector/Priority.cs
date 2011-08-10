using System;
using System.ComponentModel;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// í ímóDêÊìxÅB
    /// </summary>
    /// <remarks>
    /// The priority of a notification can be used to change the way the notification is handled
    /// and presented to the end user. For instance, higher priority notifications might be displayed
    /// with a red color or exclamation icon. However, each display is responsible for
    /// handling changes related to priority and may not make any distinction between different priority
    /// levels. Further, although each notification can request its own priority, the end user may elect 
    /// to override this priority setting, so the notification's requested priority is not guaranteed.
    /// </remarks>
    [Description("í ímóDêÊìx")]
    public enum Priority
    {
        /// <summary>Ç‰Ç¡Ç≠ÇË</summary>
        [Description("Ç‰Ç¡Ç≠ÇË")]
        VeryLow = -2,

        /// <summary>çTÇ¶Çﬂ</summary>
        [Description("çTÇ¶Çﬂ")]
        Moderate = -1,

        /// <summary>í èÌ</summary>
        [Description("í èÌ")]
        Normal = 0,

        /// <summary>óDêÊ</summary>
        [Description("óDêÊ")]
        High = 1,

        /// <summary>ãŸã}</summary>
        [Description("ãŸã}")]
        Emergency = 2
    }
}
