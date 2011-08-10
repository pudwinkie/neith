using System;
using System.Collections.Generic;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents an Error response
    /// </summary>
    public class Error : ExtensibleObject, IError
    {
        /// <summary>
        /// The error code of the response
        /// </summary>
        private int errorCode = 0;

        /// <summary>
        /// The error description of the response
        /// </summary>
        private string description;

        /// <summary>
        /// Creates a new instance of the <see cref="Error"/> class
        /// without setting the error code or description.
        /// </summary>
        protected Error()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="description">The error description</param>
        public Error(int errorCode, string description)
        {
            this.errorCode = errorCode;
            this.description = description;
        }

        /// <summary>
        /// Gets the error code of the response
        /// </summary>
        /// <value>int</value>
        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }

        /// <summary>
        /// Gets the error description of the response
        /// </summary>
        /// <value>string</value>
        public string ErrorDescription
        {
            get
            {
                return this.description;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Error"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Error"/></returns>
        public static IError FromHeaders(HeaderCollection headers)
        {
            int errorCode = headers.GetHeaderIntValue(HeaderKeys.ERROR_CODE, true);
            string description = headers.GetHeaderStringValue(HeaderKeys.ERROR_DESCRIPTION, false);

            Error error = new Error(errorCode, description);
            error.SetInhertiedAttributesFromHeaders(headers);
            return error;
        }
    }
}
