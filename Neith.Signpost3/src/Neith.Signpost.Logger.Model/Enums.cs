using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Signpost.Logger.Model
{
    /// <summary>値の更新モード</summary>
    public enum ValueUpdateMode
    {
        /// <summary>値は新たに置き換えられます。</summary>
        Swap,

        /// <summary>値は加算されます。</summary>
        Increment,

    }
}
