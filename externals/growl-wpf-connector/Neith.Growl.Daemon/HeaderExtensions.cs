using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>Header関係の拡張</summary>
    public static class HeaderExtensions
    {
        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this ISubscriptionResponse obj)
        {
            var headers = new HeaderCollection();

            var hTTL = new Header(HeaderKeys.SUBSCRIPTION_TTL, obj.TTL.ToString());
            headers.AddHeader(hTTL);

            var baseHeaders = ((IResponse)obj).ToHeaders();
            headers.AddHeaders(baseHeaders);

            return headers;
        }

        /// <summary>
        /// HeaderCollectionに変換します。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection ToHeaders(this ISubscriber obj)
        {
            var hID = new Header(HeaderKeys.SUBSCRIBER_ID, obj.ID);
            var hName = new Header(HeaderKeys.SUBSCRIBER_NAME, obj.Name);

            var headers = new HeaderCollection();
            headers.AddHeader(hID);
            headers.AddHeader(hName);
            // only pass the port if different than the standard port
            if (obj.Port != GrowlConnector.TCP_PORT) {
                var hPort = new Header(HeaderKeys.SUBSCRIBER_PORT, obj.Port.ToString());
                headers.AddHeader(hPort);
            }

            obj.AddInheritedAttributesToHeaders(headers);
            return headers;
        }



    }
}