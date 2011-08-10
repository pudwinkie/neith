using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents the base class for types that can be represented as a set of headers (including
    /// pre-defined and custom headers)
    /// </summary>
    public class ExtensibleObject : IExtensibleObject
    {
        private static string defaultMachineName = Environment.MachineName;
        private static string defaultSoftwareName = "GrowlConnector";
        private static string defaultSoftwareVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static string defaultPlatformName = Environment.OSVersion.ToString();
        private static string defaultPlatformVersion = Environment.OSVersion.Version.ToString();

        /*
        Origin-Machine-Name: Gazebo
        Origin-Software-Name: GrowlAIRConnector
        Origin-Software-Version: 2.0.3230.22943
        Origin-Platform-Name: Microsoft Windows XP
        Origin-Platform-Version: 5.2.3790.131072
         * */

        //private string requestID;
        private string machineName;
        private string softwareName;
        private string softwareVersion;
        private string platformName;
        private string platformVersion;

        Dictionary<string, string> customTextAttributes;
        Dictionary<string, Resource> customBinaryAttributes;

        /// <summary>
        /// Creates a new instance of the <see cref="ExtensibleObject"/> class.
        /// </summary>
        public ExtensibleObject()
        {
            this.machineName = defaultMachineName;
            this.softwareName = defaultSoftwareName;
            this.softwareVersion = defaultSoftwareVersion;
            this.platformName = defaultPlatformName;
            this.platformVersion = defaultPlatformVersion;

            this.customTextAttributes = new Dictionary<string, string>();
            this.customBinaryAttributes = new Dictionary<string, Resource>();
        }

        /// <summary>
        /// The name of the machine sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: Gazebo
        /// </value>
        public string MachineName { get { return machineName; } set { machineName = value; } }

        /// <summary>
        /// The name of the software (framework) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: GrowlConnector
        /// </value>
        public string SoftwareName { get { return softwareName; } set { softwareName = value; } }

        /// <summary>
        /// The version of the software (framework) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: 2.0
        /// </value>
        public string SoftwareVersion { get { return softwareVersion; } set { softwareVersion = value; } }

        /// <summary>
        /// The name of the platform (OS) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: Windows XP
        /// </value>
        public string PlatformName { get { return platformName; } set { platformName = value; } }

        /// <summary>
        /// The version of the platform (OS) sending the notification
        /// </summary>
        /// <value>
        /// string - Ex: 5.0.12
        /// </value>
        public string PlatformVersion { get { return platformVersion; } set { platformVersion = value; } }

        /// <summary>
        /// Gets a collection of custom text attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom text attribute is equivalent to a custom "X-" header
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, string> CustomTextAttributes
        {
            get
            {
                return this.customTextAttributes;
            }
        }

        /// <summary>
        /// Gets a collection of custom binary attributes associated with this object
        /// </summary>
        /// <remarks>
        /// Each custom binary attribute is equivalent to a custom "X-" header with a 
        /// "x-growl-resource://" value, as well as the necessary resource headers
        /// (Identifier, Length, and binary bytes)
        /// </remarks>
        /// <value>
        /// <see cref="Dictionary{TKey, TVal}"/>
        /// </value>
        public Dictionary<string, Resource> CustomBinaryAttributes
        {
            get
            {
                return this.customBinaryAttributes;
            }
        }


        /// <summary>
        /// Sets the software information (name/version) for the current application
        /// </summary>
        /// <param name="name">The name of the software</param>
        /// <param name="version">The version of the software</param>
        /// <remarks>
        /// This method is typically called by a server implementation that wants to identify itself
        /// properly in the 'Origin-Software-*' headers.
        /// </remarks>
        public static void SetSoftwareInformation(string name, string version)
        {
            defaultSoftwareName = name;
            defaultSoftwareVersion = version;
        }

        /// <summary>
        /// Sets the platform information (name/version) for the current application
        /// </summary>
        /// <param name="name">The name of the platform</param>
        /// <param name="version">The version of the platform</param>
        /// <remarks>
        /// This method is typically called by a server implementation that wants to identify itself
        /// properly in the 'Origin-Platform-*' headers.
        /// Normally it is not necessary to call this method as the platform information is automatically
        /// detected.
        /// </remarks>
        public static void SetPlatformInformation(string name, string version)
        {
            defaultPlatformName = name;
            defaultPlatformVersion = version;
        }
    }
}