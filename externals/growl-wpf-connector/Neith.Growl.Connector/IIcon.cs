using System;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>アイコンリソース</summary>
    public interface IIcon
    {
        /// <summary>アイコン</summary>
        Resource Icon { get; set; }

        /// <summary>名称</summary>
        string Name { get; set; }
    }
}
