using System;
namespace Neith.Growl.Connector
{
    /// <summary>コールバック基本情報</summary>
    public interface ICallbackDataBase
    {
        /// <summary>データ名</summary>
        string Data { get; }

        /// <summary>タイプ名</summary>
        string Type { get; }
    }
}
