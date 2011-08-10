using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// CallbackContext拡張
    /// </summary>
    public static class CallbackContextExtensions
    {
        /// <summary>
        /// コールバックのためにコネクションを維持する必要があるならtrueを返します。
        /// </summary>
        /// <returns>
        /// <c>true</c> if the connection needs to be kept open,
        /// <c>false</c> if the connection can be closed (url callback)
        /// </returns>
        public static bool ShouldKeepConnectionOpen(this ICallbackContext obj)
        {
            if (!String.IsNullOrEmpty(obj.Data) && !String.IsNullOrEmpty(obj.Type) &&
                (String.IsNullOrEmpty(obj.CallbackUrl))) {
                return true;
            }
            return false;
        }


    }
}
