using System;
using System.Collections.Generic;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>エラー情報</summary>
    public interface IError : IExtensibleObject
    {
        /// <summary>エラーコード</summary>
        int ErrorCode { get; }

        /// <summary>エラー説明</summary>
        string ErrorDescription { get; }
    }
}
