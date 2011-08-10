using System;
using System.Collections.Generic;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>基本情報</summary>
    public interface IExtensibleObject
    {
        /// <summary>カスタムバイナリ属性</summary>
        Dictionary<string, Resource> CustomBinaryAttributes { get; }

        /// <summary>カスタム文字列属性</summary>
        Dictionary<string, string> CustomTextAttributes { get; }

        /// <summary>マシン名</summary>
        string MachineName { get; set; }

        /// <summary>プラットフォーム(OS)名</summary>
        string PlatformName { get; set; }

        /// <summary>プラットフォーム(OS)バージョン</summary>
        string PlatformVersion { get; set; }

        /// <summary>ソフトウェア名</summary>
        string SoftwareName { get; set; }

        /// <summary>ソフトウェアバージョン</summary>
        string SoftwareVersion { get; set; }
    }
}
