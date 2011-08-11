using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    public static class CallbackInfoExtensions
    {
        /// <summary>
        /// Indicates if the server should keep the connection open to perform the callback
        /// </summary>
        /// <returns>
        /// <c>true</c> to keep the connection open and perform the callback via the connection,
        /// <c>false</c> if the callback is url-based and will be performed out-of-band
        /// </returns>
        public static bool ShouldKeepConnectionOpen(this CallbackInfo item)
        {
            if (item.Context != null && item.Context.ShouldKeepConnectionOpen())
                return true;
            else
                return false;
        }

    }
}
