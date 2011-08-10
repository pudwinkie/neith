using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.Connector;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// Represents a client that is subscribed to receive notifications from a Growl server.
    /// </summary>
    [Serializable]
    public class Subscriber : ExtensibleObject, ISubscriber  // TODO: custom serialization
    {
        /// <summary>
        /// The unique id of the subscriber
        /// </summary>
        private string id;

        /// <summary>
        /// The friendly name of the subscriber
        /// </summary>
        private string name;

        /// <summary>
        /// The port the subscriber will listen on
        /// </summary>
        private int port;

        /// <summary>
        /// The IP address of the subscriber
        /// </summary>
        private string ipaddress;

        /// <summary>
        /// The <see cref="Key"/> used to authenticate and encrypt messages
        /// </summary>
        private SubscriberKey key;

        /// <summary>
        /// Creates a new instance of the <see cref="Subscriber"/> class.
        /// </summary>
        /// <param name="id">The unique ID of the subscriber</param>
        /// <param name="name">The identifying name of the subscriber</param>
        public Subscriber(string id, string name)
            : this(id, name, GrowlConnector.TCP_PORT)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Subscriber"/> class.
        /// </summary>
        /// <param name="id">The unique ID of the subscriber</param>
        /// <param name="name">The identifying name of the subscriber</param>
        /// <param name="port">The port the subscriber will listen on</param>
        public Subscriber(string id, string name, int port)
        {
            this.id = id;
            this.name = name;
            this.port = port;
        }

        /// <summary>
        /// The unique ID of the subscriber
        /// </summary>
        /// <value>
        /// guid
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
        /// The identifying name of the subscriber
        /// </summary>
        /// <value>
        /// string - Ex: Growl on Gazebo
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
        /// The port that the client will listen on
        /// </summary>
        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        /// <summary>
        /// The IP address of the client
        /// </summary>
        /// <remarks>
        /// This value is read-only and is set by the subscribed-to Growl server.
        /// </remarks>
        public string IPAddress
        {
            get
            {
                return this.ipaddress;
            }
            set
            {
                this.ipaddress = value;
            }
        }

        /// <summary>
        /// The <see cref="Key"/> used to authorize and encrypt messages
        /// </summary>
        /// <value><see cref="SubscriberKey"/></value>
        public SubscriberKey Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = value;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Subscriber"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Subscriber"/></returns>
        public static ISubscriber FromHeaders(HeaderCollection headers)
        {
            string id = headers.GetHeaderStringValue(HeaderKeys.SUBSCRIBER_ID, true);
            string name = headers.GetHeaderStringValue(HeaderKeys.SUBSCRIBER_NAME, true);
            int port = headers.GetHeaderIntValue(HeaderKeys.SUBSCRIBER_PORT, false);
            if (port == 0) port = GrowlConnector.TCP_PORT;

            var subscriber = new Subscriber(id, name, port);

            subscriber.SetInhertiedAttributesFromHeaders(headers);

            return subscriber;
        }
    }
}
