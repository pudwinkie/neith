using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents the information needed to perform a callback to the notifying application
    /// </summary>
    public class CallbackContext : CallbackDataBase, ICallbackContext
    {
        /// <summary>
        /// The callback url
        /// </summary>
        private string url;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackContext"/> class.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="type">The type.</param>
        public CallbackContext(string data, string type) : base(data, type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackContext"/> class,
        /// specifying a callback url.
        /// </summary>
        /// <param name="url">The URL.</param>
        public CallbackContext(string url)
        {
            this.url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackContext"/> class.
        /// </summary>
        private CallbackContext()
        {
        }

        /// <summary>
        /// Gets the callback URL.
        /// </summary>
        /// <value>The callback URL.</value>
        public string CallbackUrl
        {
            get
            {
                return this.url;
            }
        }

        /// <summary>
        /// Creates a new <see cref="CallbackContext"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="CallbackContext"/></returns>
        public new static ICallbackContext FromHeaders(HeaderCollection headers)
        {
            var baseObj = CallbackDataBase.FromHeaders(headers);

            var context = new CallbackContext(baseObj.Data, baseObj.Type);

            return context;
        }
    }
}
