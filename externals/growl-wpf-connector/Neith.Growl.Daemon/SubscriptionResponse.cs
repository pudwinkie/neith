using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// Represents a GNTP reponse to a SUBSCRIBE request
    /// </summary>
    public class SubscriptionResponse : Response, ISubscriptionResponse
    {
        /// <summary>
        /// The amount of time (in seconds) that the subscription is valid for
        /// </summary>
        private int ttl;


        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionResponse"/> class.
        /// </summary>
        /// <param name="ttl">The amount of time (in seconds) that the subscription is valid for</param>
        public SubscriptionResponse(int ttl)
            : base()
        {
            this.ttl = ttl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionResponse"/> class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorDescription">The error description.</param>
        private SubscriptionResponse(int errorCode, string errorDescription)
            : base(errorCode, errorDescription)
        {
        }

        /// <summary>
        /// Gets the amount of time (in seconds) that the subscription is valid for
        /// </summary>
        /// <remarks>
        /// This value generally should not be less than 60 seconds in order to give the
        /// subscriber time to process any notifications and renew its subscriptions.
        /// </remarks>
        public int TTL
        {
            get
            {
                return this.ttl;
            }
        }

        /// <summary>
        /// Creates a new <see cref="SubscriptionResponse"/> from a base <see cref="Response"/> and a list of headers
        /// </summary>
        /// <param name="response">The base <see cref="Response"/></param>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the response</param>
        /// <returns><see cref="SubscriptionResponse"/></returns>
        internal static ISubscriptionResponse FromResponse(IResponse response, HeaderCollection headers)
        {
            SubscriptionResponse sr = null;
            if (response.IsOK)
            {
                int ttl = headers.GetHeaderIntValue(HeaderKeys.SUBSCRIPTION_TTL, true);

                sr = new SubscriptionResponse(ttl);
            }
            else
            {
                sr = new SubscriptionResponse(response.ErrorCode, response.ErrorDescription);
                sr.ttl = 0;
            }

            sr.SetInhertiedAttributesFromHeaders(headers);
            return sr;
        }
    }
}
