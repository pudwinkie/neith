using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Logger.Model
{
    /// <summary>
    /// ロギングモジュールの基本インターフェース。
    /// </summary>
    public interface ILoggerModule : IDisposable
    {   
        /// <summary>モジュール名</summary>
        string Name { get; }


    }
}
