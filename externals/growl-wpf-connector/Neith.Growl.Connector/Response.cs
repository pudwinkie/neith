using System;
using System.Collections.Generic;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents a GNTP response
    /// </summary>
    public class Response : Error, IResponse
    {
        /// <summary>
        /// Indicates if this is an OK response
        /// </summary>
        private bool isOK;

        /// <summary>
        /// Indicates what type of request this is in response to
        /// </summary>
        private string inResponseTo;

        /// <summary>
        /// Contains the callback information and result
        /// </summary>
        private ICallbackData callbackData;

        /// <summary>
        /// Contains the returned <see cref="RequestData"/>
        /// </summary>
        private RequestData requestData;

        /// <summary>
        /// Creates a new instance of the <see cref="Response"/> class,
        /// setting the IsOK property to <c>true</c>.
        /// </summary>
        public Response()
            : base()
        {
            this.IsOK = true;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Response"/> class,
        /// setting the ErrorCode and ErrorDescription properties.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorDescription">The error description</param>
        public Response(int errorCode, string errorDescription)
            : base(errorCode, errorDescription)
        {
            this.IsOK = false;
        }

        /// <summary>
        /// Gets or sets a flag that indicates if this is an OK response
        /// </summary>
        /// <value>
        /// <c>true</c> if this is an OK or CALLBACK response,
        /// <c>false</c> if this is an ERROR response
        /// </value>
        public bool IsOK
        {
            get
            {
                return this.isOK;
            }
            set
            {
                this.isOK = value;
            }
        }

        /// <summary>
        /// Gets a flag that indicates if this is an ERROR response
        /// </summary>
        /// <value>
        /// <c>true</c> if this is an ERROR response,
        /// <c>false</c> if this is any other response
        /// </value>
        public bool IsError
        {
            get
            {
                return !this.IsOK;
            }
        }

        /// <summary>
        /// Gets a flag that indicates if this is a CALLBACK response
        /// </summary>
        /// <value>
        /// <c>true</c> if this is a CALLBACK response
        /// <c>false</c> if this is any other response
        /// </value>
        public bool IsCallback
        {
            get
            {
                if (this.callbackData != null)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="CallbackData"/> if this is a callback-type response
        /// </summary>
        /// <value><see cref="CallbackData"/></value>
        public ICallbackData CallbackData { get { return callbackData; } set { callbackData = value; } }

        /// <summary>
        /// Gets or sets the type of request that this response is in response to.
        /// </summary>
        /// <value>string</value>
        public string InResponseTo { get { return inResponseTo; } set { inResponseTo = value; } }

        /// <summary>
        /// Gets the <see cref="RequestData"/> associated with this transaction
        /// </summary>
        /// <value><see cref="RequestData"/></value>
        public RequestData RequestData { get { return requestData; } set { requestData = value; } }

        /// <summary>
        /// Sets the <see cref="CallbackData"/> for this response
        /// </summary>
        /// <param name="notificationID">The ID of the notification making the callback</param>
        /// <param name="callbackContext">The <see cref="ICallbackContext"/> of the request</param>
        /// <param name="callbackResult">The <see cref="CallbackResult"/> (clicked, closed)</param>
        public void SetCallbackData(string notificationID, ICallbackContext callbackContext, CallbackResult callbackResult)
        {
            if (callbackContext != null) {
                var cd = new CallbackData(callbackContext.Data, callbackContext.Type, callbackResult, notificationID);
                this.callbackData = cd;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Response"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the response</param>
        /// <returns><see cref="Response"/></returns>
        public new static IResponse FromHeaders(HeaderCollection headers)
        {
            var errorCode = headers.GetHeaderIntValue(HeaderKeys.ERROR_CODE, true);
            var description = headers.GetHeaderStringValue(HeaderKeys.ERROR_DESCRIPTION, false);

            var response = new Response(errorCode, description);
            response.SetInhertiedAttributesFromHeaders(headers);
            return response;
        }

    }
}