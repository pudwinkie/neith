using System;
using System.ComponentModel;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// �ʒm�D��x�B
    /// </summary>
    /// <remarks>
    /// The priority of a notification can be used to change the way the notification is handled
    /// and presented to the end user. For instance, higher priority notifications might be displayed
    /// with a red color or exclamation icon. However, each display is responsible for
    /// handling changes related to priority and may not make any distinction between different priority
    /// levels. Further, although each notification can request its own priority, the end user may elect 
    /// to override this priority setting, so the notification's requested priority is not guaranteed.
    /// </remarks>
    [Description("�ʒm�D��x")]
    public enum Priority
    {
        /// <summary>�������</summary>
        [Description("�������")]
        VeryLow = -2,

        /// <summary>�T����</summary>
        [Description("�T����")]
        Moderate = -1,

        /// <summary>�ʏ�</summary>
        [Description("�ʏ�")]
        Normal = 0,

        /// <summary>�D��</summary>
        [Description("�D��")]
        High = 1,

        /// <summary>�ً}</summary>
        [Description("�ً}")]
        Emergency = 2
    }
}
