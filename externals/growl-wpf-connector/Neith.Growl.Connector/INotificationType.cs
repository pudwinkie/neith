using System;
using System.Collections.Generic;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>通知タイプ情報</summary>
    public interface INotificationType : IExtensibleObject, IIcon
    {
        /// <summary>表示名</summary>
        string DisplayName { get; set; }

        /// <summary>有効ならtrue</summary>
        bool Enabled { get; set; }
    }
}
