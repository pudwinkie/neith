using System;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents a notification
    /// </summary>
    public class Notification : ExtensibleObject, INotification
    {
        /// <summary>
        /// The name of the application sending the notification
        /// </summary>
        private string applicationName;

        /// <summary>
        /// The name (type) of the notification
        /// </summary>
        private string name;

        /// <summary>
        /// A unique id for the notification (sender-specified)
        /// </summary>
        private string id;

        /// <summary>
        /// The notification title
        /// </summary>
        private string title;

        /// <summary>
        /// The notification text
        /// </summary>
        private string text;

        /// <summary>
        /// Indicates if the notification should be sticky or not
        /// </summary>
        private bool sticky = false;

        /// <summary>
        /// The notification priority
        /// </summary>
        private Priority priority = Neith.Growl.Connector.Priority.Normal;

        /// <summary>
        /// The notification icon
        /// </summary>
        private Resource icon;

        /// <summary>
        /// The coalescing (grouping) id
        /// </summary>
        private string coalescingID;


        /// <summary>
        /// Creates a instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application sending the notification</param>
        /// <param name="notificationName">The notification name (type)</param>
        /// <param name="id">A unique ID for the notification</param>
        /// <param name="title">The notification title</param>
        /// <param name="text">The notification text</param>
        public Notification(string applicationName, string notificationName, string id, string title, string text)
            : this(applicationName, notificationName, id, title, text, null, false, Priority.Normal, null)
        {
        }

        /// <summary>
        /// Creates a instance of the <see cref="Notification"/> class.
        /// </summary>
        /// <param name="applicationName">The name of the application sending the notification</param>
        /// <param name="notificationName">The notification name (type)</param>
        /// <param name="id">A unique ID for the notification</param>
        /// <param name="title">The notification title</param>
        /// <param name="text">The notification text</param>
        /// <param name="icon">A <see cref="Resource"/> for the icon associated with the notification</param>
        /// <param name="sticky"><c>true</c> to suggest that the notification should be sticky;<c>false</c> otherwise</param>
        /// <param name="priority">The <see cref="Priority"/> of the notification</param>
        /// <param name="coalescingID">The coalescing (grouping) ID (used to replace exisiting notifications)</param>
        public Notification(string applicationName, string notificationName, string id, string title, string text, Resource icon, bool sticky, Priority priority, string coalescingID)
        {
            this.applicationName = applicationName;
            this.name = notificationName;
            this.id = id;
            this.title = title;
            this.text = text;
            this.icon = icon;
            this.sticky = sticky;
            this.priority = priority;
            this.coalescingID = coalescingID;
        }

        /// <summary>
        /// The name of the application sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: SurfWriter
        /// </value>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }

        /// <summary>
        /// The name (type) of the notification.
        /// </summary>
        /// <value>
        /// string - This should match the name of one of the registered <see cref="INotificationType"/>s
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
        /// A unique ID for the notification
        /// </summary>
        /// <value>
        /// string - This value is assigned by the sending application and can be any arbitrary string. This value is optional.
        /// </value>
        public string ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        /// <summary>
        /// The title of the notification
        /// </summary>
        /// <value>
        /// string - Ex: Download Complete
        /// </value>
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
            }
        }

        /// <summary>
        /// The text of the notification
        /// </summary>
        /// <value>
        /// string - Ex: The file 'filename.txt' had finished downloading
        /// </value>
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        /// <summary>
        /// Indicates if the notification should be sticky or not.
        /// </summary>
        /// <value>
        /// <c>true</c> to suggest that the notification should be sticky;
        /// <c>false</c> otherwise
        /// </value>
        public bool Sticky
        {
            get
            {
                return this.sticky;
            }
            set
            {
                this.sticky = value;
            }
        }

        /// <summary>
        /// The priority of the notification
        /// </summary>
        /// <value>
        /// <see cref="Priority"/>
        /// </value>
        public Priority Priority
        {
            get
            {
                return this.priority;
            }
            set
            {
                this.priority = value;
            }
        }

        /// <summary>
        /// The icon to associate with this notification
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
        /// An ID used to group notifications.
        /// </summary>
        /// <value>
        /// string - This value is assigned by the sending application and can be any arbitrary string. This value is optional.
        /// </value>
        /// <remarks>
        /// 'Coalescing' in Growl is actually referring to replacement. If a previously sent notification is still on-screen, it
        /// can be updated/replaced by specifying the same CoalescingID.
        /// </remarks>
        public string CoalescingID
        {
            get
            {
                return this.coalescingID;
            }
            set
            {
                this.coalescingID = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Notification"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Notification"/></returns>
        public static INotification FromHeaders(HeaderCollection headers)
        {
            var appName = headers.GetHeaderStringValue(HeaderKeys.APPLICATION_NAME, true);
            var name = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_NAME, true);
            var id = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_ID, false);
            var title = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_TITLE, true);
            var text = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_TEXT, false);
            if (text == null) text = String.Empty;
            var coalescingID = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_COALESCING_ID, false);
            var icon = headers.GetHeaderResourceValue(HeaderKeys.NOTIFICATION_ICON, false);
            var sticky = headers.GetHeaderBooleanValue(HeaderKeys.NOTIFICATION_STICKY, false);
            var p = headers.GetHeaderStringValue(HeaderKeys.NOTIFICATION_PRIORITY, false);
            var priority = Neith.Growl.Connector.Priority.Normal;
            if (p != null) {
                int pval = 0;
                bool pok = int.TryParse(p, out pval);
                if (pok && Enum.IsDefined(typeof(Priority), pval))
                    priority = (Priority)pval;
            }

            var notification = new Notification(appName, name, id, title, text, icon, sticky, priority, coalescingID);
            notification.SetInhertiedAttributesFromHeaders(headers);
            return notification;
        }
    }
}
