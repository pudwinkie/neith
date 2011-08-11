using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// Represents information needed by the receiver in order to perform a callback
    /// </summary>
    public class CallbackInfo
    {
        /// <summary>
        /// Handles the <see cref="CallbackInfo.ForwardedNotificationCallback"/> event
        /// </summary>
        public delegate void ForwardedNotificationCallbackHandler(IResponse response, CallbackData callbackData, CallbackInfo callbackInfo);

        /// <summary>
        /// Occurs when a forwarded notification triggers a callback from the forwarded destination
        /// </summary>
        [field: NonSerialized]
        public event ForwardedNotificationCallbackHandler ForwardedNotificationCallback;


        /// <summary>
        /// The callback context from the request
        /// </summary>
        public ICallbackContext Context { get; set; }

        /// <summary>
        /// The MessageHandler that will peform the callback write
        /// </summary>
        public MessageHandler MessageHandler { get; set; }

        /// <summary>
        /// Gets or sets the unique notification ID provided in the request
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string NotificationID { get; set; }

        /// <summary>
        /// Indicates if the request that spawned this callback has already been responded to.
        /// </summary>
        /// <remarks>
        /// When a notification is forwarded to another computer, the notification may be clicked/handled on both computers.
        /// Only the first response action is returned and all subsequent actions are ignored.
        /// </remarks>
        public bool AlreadyResponded { get; set; }

        /// <summary>
        /// Gets a list of all extended-information key/value pairs that should be returned with the callback response.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; }

        /// <summary>
        /// Represents metadata about a received request such as when it was received, by whom, etc.
        /// </summary>
        public IRequestInfo RequestInfo { get; set; }

        /// <summary>
        /// Handles the callback from a forwarder.
        /// </summary>
        /// <param name="response">The <see cref="IResponse"/> from the forwarder</param>
        /// <param name="callbackData">The <see cref="CallbackData"/></param>
        public void HandleCallbackFromForwarder(IResponse response, CallbackData callbackData)
        {
            this.RequestInfo.SaveHandlingInfo(String.Format("Was responded to on {0} - Action: {1}", response.MachineName, callbackData.Result));

            if (this.ForwardedNotificationCallback != null) {
                this.ForwardedNotificationCallback(response, callbackData, this);
            }
        }


    }
}
