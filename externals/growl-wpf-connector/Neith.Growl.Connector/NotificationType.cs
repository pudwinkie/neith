using System;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents a type of notification that an application may send
    /// </summary>
    public class NotificationType : ExtensibleObject, INotificationType
    {
        /// <summary>
        /// The name of the notification type
        /// </summary>
        private string name = "Undefined Notification";

        /// <summary>
        /// The display name of the notification type
        /// </summary>
        private string displayName = null;

        /// <summary>
        /// The default icon for notifications of this type
        /// </summary>
        private Resource icon;

        /// <summary>
        /// Indicates if this type of notification should be enabled or disabled by default
        /// </summary>
        private bool enabled = true;

        /// <summary>
        /// Creates a instance of the <see cref="NotificationType"/> class.
        /// </summary>
        /// <param name="name">The name of this type of notification</param>
        public NotificationType(string name)
            : this(name, null, null, true)
        {
        }

        /// <summary>
        /// Creates a instance of the <see cref="NotificationType"/> class.
        /// </summary>
        /// <param name="name">The name of this type of notification</param>
        /// <param name="displayName">The display name of this type of notification</param>
        public NotificationType(string name, string displayName)
            : this(name, displayName, null, true)
        {
        }

        /// <summary>
        /// Creates a instance of the <see cref="NotificationType"/> class.
        /// </summary>
        /// <param name="name">The name of this type of notification</param>
        /// <param name="displayName">The display name of this type of notification</param>
        /// <param name="icon">The default icon for notifications of this type</param>
        /// <param name="enabled"><c>true</c> if this type of notification should be enabled by default; <c>false</c> if this type of notification should be disabled by default</param>
        public NotificationType(string name, string displayName, Resource icon, bool enabled)
        {
            this.name = name;
            this.displayName = displayName;
            this.icon = icon;
            this.enabled = enabled;
        }

        /// <summary>
        /// The name of this type of notification
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        /// <summary>
        /// The display name of this type of notification
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string DisplayName
        {
            get
            {
                return (this.displayName == null ? this.name : this.displayName);
            }
            set
            {
                this.displayName = value;
            }
        }

        /// <summary>
        /// The default icon for notifications of this type
        /// </summary>
        /// <value>
        /// <see cref="Resource"/>
        /// </value>
        public Resource Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = value;
            }
        }

        /// <summary>
        /// Indicates if this type of notification should be enabled or disabled by default
        /// </summary>
        /// <value>
        /// <c>true</c> if this type of notification should be enabled by default;
        /// <c>false</c> if this type of notification should be disabled by default
        /// </value>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                this.enabled = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="INotificationType"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the response</param>
        /// <returns><see cref="INotificationType"/></returns>
        public static INotificationType FromHeaders(HeaderCollection headers)
        {
            var name = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_NAME, true);
            var displayName = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_DISPLAY_NAME, false);
            var icon = headers.GetHeaderResourceValue(HeaderKeys.NOTIFICATION_ICON, false);
            var enabled = headers.GetHeaderBooleanValue(HeaderKeys.NOTIFICATION_ENABLED, false);

            var nt = new NotificationType(name, displayName, icon, enabled);
            nt.SetCustomAttributesFromHeaders(headers);    // NOTE: dont call SetInheritedAttributesFromHeaders because we want to ignore the common attributes
            return nt;
        }
    }
}