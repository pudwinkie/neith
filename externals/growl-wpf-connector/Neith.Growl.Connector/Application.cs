using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents an application that may send notifications
    /// </summary>
    public class Application : ExtensibleObject, IApplication
    {
        /// <summary>
        /// The application name
        /// </summary>
        private string name;

        /// <summary>
        /// The application's icon
        /// </summary>
        private Resource icon;


        /// <summary>
        /// Creates a new instance of the <see cref="Application"/> class.
        /// </summary>
        /// <param name="name">The name of the application</param>
        public Application(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// The name of the application
        /// </summary>
        /// <value>
        /// string - Ex: SurfWriter
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
        /// The application's icon
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
        /// Creates a new <see cref="Application"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="Application"/></returns>
        public static IApplication FromHeaders(HeaderCollection headers)
        {
            var name = headers.GetHeaderStringValue(HeaderKeys.APPLICATION_NAME, true);
            var icon = headers.GetHeaderResourceValue(HeaderKeys.APPLICATION_ICON, false);

            var app = new Application(name);
            app.Icon = icon;

            app.SetInhertiedAttributesFromHeaders(headers);

            return app;
        }
    }
}
