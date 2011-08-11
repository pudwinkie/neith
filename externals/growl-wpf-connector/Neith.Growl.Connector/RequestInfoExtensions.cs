using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// RequestInfo拡張
    /// </summary>
    public static class RequestInfoExtensions
    {
        /// <summary>
        /// Saves arbitrary information about how the notification was handled.
        /// </summary>
        /// <param name="info">The information to save</param>
        /// <remarks>
        /// The handling information saved is primarily used for writing to the log file (if enabled)
        /// </remarks>
         public static void SaveHandlingInfo(this IRequestInfo src, string info)
        {
            src.HandlingInfo.Add(info);
        }

        /// <summary>
        /// Indicates if the request was forwarded from another machine
        /// </summary>
        /// <returns><c>true</c> if the request was forwarded from another machine;<c>false</c> otherwise</returns>
         public static bool WasForwarded(this IRequestInfo src)
        {
            if (src.PreviousReceivedHeaders != null && src.PreviousReceivedHeaders.Count > 0) {
                return true;
            }
            else
                return false;
        }
    }
}
