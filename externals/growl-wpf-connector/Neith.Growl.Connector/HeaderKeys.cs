using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// HeaderのKey定義
    /// </summary>
    public static class HeaderKeys
    {
        /// <summary>
        /// Response-Action header
        /// </summary>
        public const string RESPONSE_ACTION = "Response-Action";
        /// <summary>
        /// Application-Name header
        /// </summary>
        public const string APPLICATION_NAME = "Application-Name";
        /// <summary>
        /// Application-Icon header
        /// </summary>
        public const string APPLICATION_ICON = "Application-Icon";
        /// <summary>
        /// Notifications-Count header
        /// </summary>
        public const string NOTIFICATIONS_COUNT = "Notifications-Count";
        /// <summary>
        /// Notification-Name header
        /// </summary>
        public const string NOTIFICATION_NAME = "Notification-Name";
        /// <summary>
        /// Notification-Display-Name header
        /// </summary>
        public const string NOTIFICATION_DISPLAY_NAME = "Notification-Display-Name";
        /// <summary>
        /// Notification-Enabled header
        /// </summary>
        public const string NOTIFICATION_ENABLED = "Notification-Enabled";
        /// <summary>
        /// Notification-Icon header
        /// </summary>
        public const string NOTIFICATION_ICON = "Notification-Icon";
        /// <summary>
        /// Notification-ID header
        /// </summary>
        public const string NOTIFICATION_ID = "Notification-ID";
        /// <summary>
        /// Notification-Title header
        /// </summary>
        public const string NOTIFICATION_TITLE = "Notification-Title";
        /// <summary>
        /// Notification-Text header
        /// </summary>
        public const string NOTIFICATION_TEXT = "Notification-Text";
        /// <summary>
        /// Notification-Sticky header
        /// </summary>
        public const string NOTIFICATION_STICKY = "Notification-Sticky";
        /// <summary>
        /// Notification-Priority header
        /// </summary>
        public const string NOTIFICATION_PRIORITY = "Notification-Priority";
        /// <summary>
        /// Notification-Coalescing-ID header
        /// </summary>
        public const string NOTIFICATION_COALESCING_ID = "Notification-Coalescing-ID";
        /// <summary>
        /// Notification-Callback-Result header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_RESULT = "Notification-Callback-Result";
        /// <summary>
        /// Notification-Callback-Timestamp header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_TIMESTAMP = "Notification-Callback-Timestamp";
        /// <summary>
        /// Notification-Callback-Context header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT = "Notification-Callback-Context";
        /// <summary>
        /// Notification-Callback-Context-Type header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT_TYPE = "Notification-Callback-Context-Type";
        /// <summary>
        /// Notification-Callback-Target header
        /// </summary>
        public const string NOTIFICATION_CALLBACK_TARGET = "Notification-Callback-Target";
        /// <summary>
        /// Notification-Callback-Context-Target header (this is not a valid header, but it is left in for compatibility with existing implementations)
        /// </summary>
        public const string NOTIFICATION_CALLBACK_CONTEXT_TARGET = "Notification-Callback-Context-Target";
        /// <summary>
        /// Identifier header
        /// </summary>
        public const string RESOURCE_IDENTIFIER = "Identifier";
        /// <summary>
        /// Length header
        /// </summary>
        public const string RESOURCE_LENGTH = "Length";
        /// <summary>
        /// Origin-Machine-Name header
        /// </summary>
        public const string ORIGIN_MACHINE_NAME = "Origin-Machine-Name";
        /// <summary>
        /// Origin-Software-Name header
        /// </summary>
        public const string ORIGIN_SOFTWARE_NAME = "Origin-Software-Name";
        /// <summary>
        /// Origin-Software-Version header
        /// </summary>
        public const string ORIGIN_SOFTWARE_VERSION = "Origin-Software-Version";
        /// <summary>
        /// Origin-Platform-Name header
        /// </summary>
        public const string ORIGIN_PLATFORM_NAME = "Origin-Platform-Name";
        /// <summary>
        /// Origin-Platform-Version header
        /// </summary>
        public const string ORIGIN_PLATFORM_VERSION = "Origin-Platform-Version";
        /// <summary>
        /// Error-Code header
        /// </summary>
        public const string ERROR_CODE = "Error-Code";
        /// <summary>
        /// Error-Description header
        /// </summary>
        public const string ERROR_DESCRIPTION = "Error-Description";
        /// <summary>
        /// Received header
        /// </summary>
        public const string RECEIVED = "Received";
        /// <summary>
        /// Subscriber-ID header
        /// </summary>
        public const string SUBSCRIBER_ID = "Subscriber-ID";
        /// <summary>
        /// Subscriber-Name header
        /// </summary>
        public const string SUBSCRIBER_NAME = "Subscriber-Name";
        /// <summary>
        /// Subscriber-Port header
        /// </summary>
        public const string SUBSCRIBER_PORT = "Subscriber-Port";
        /// <summary>
        /// Subscription-TTL header
        /// </summary>
        public const string SUBSCRIPTION_TTL = "Subscription-TTL";
    }
}
