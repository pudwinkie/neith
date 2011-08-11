using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// Represents a valid parsed GNTP request
    /// </summary>
    public class GNTPRequest : IGNTPRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GNTPRequest"/> class.
        /// </summary>
        /// <param name="requestInfo">RequestInfo</param>
        /// <param name="version">The version of the GNTP request.</param>
        /// <param name="directive">The type of GNTP request.</param>
        /// <param name="key">The key used to validate and encrypt the message.</param>
        /// <param name="headers">The collection of headers parsed from the current request.</param>
        /// <param name="applicationName">The name of the application sending the request.</param>
        /// <param name="notificationsToBeRegistered">A collection of the groups of headers for each notification type to be registered.</param>
        /// <param name="callbackContext">The callback context associated with the request.</param>
        public GNTPRequest(IRequestInfo requestInfo, string version, RequestType directive, Key key, HeaderCollection headers, string applicationName, List<HeaderCollection> notificationsToBeRegistered, ICallbackContext callbackContext)
        {
            RequestInfo = requestInfo;
            Version = version;
            Directive = directive;
            Key = key;
            Headers = headers;
            ApplicationName = applicationName;
            NotificationsToBeRegistered = notificationsToBeRegistered;
            CallbackContext = callbackContext;
        }

        /// <summary>リクエストインフォメーション</summary>
        public IRequestInfo RequestInfo { get; private set; }


        /// <summary>
        /// Gets the version of the GNTP request
        /// </summary>
        /// <value>The only supported value is currently: 1.0</value>
        public string Version { get; private set; }

        /// <summary>
        /// Gets the type of the request
        /// </summary>
        /// <value><see cref="RequestType"/></value>
        public RequestType Directive { get; private set; }

        /// <summary>
        /// Gets the <see cref="Key"/> used to validate and encrypt the request
        /// </summary>
        /// <value><see cref="Key"/></value>
        public Key Key { get; private set; }

        /// <summary>
        /// Gets the list of headers parsed from the request.
        /// </summary>
        /// <value><see cref="HeaderCollection"/></value>
        public HeaderCollection Headers { get; private set; }

        /// <summary>
        /// Gets the name of the application sending the request
        /// </summary>
        /// <value>string</value>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// Gets the collection of groups of headers for all notifications to be registered.
        /// </summary>
        /// <value><see cref="List{HeaderCollection}"/></value>
        public List<HeaderCollection> NotificationsToBeRegistered { get; private set; }

        /// <summary>
        /// Gets the callback context associated with the request.
        /// </summary>
        /// <value><see cref="CallbackContext"/></value>
        public ICallbackContext CallbackContext { get; private set; }
    }
}
