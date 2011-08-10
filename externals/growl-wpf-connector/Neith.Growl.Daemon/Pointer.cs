using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.Connector;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Daemon
{
    /// <summary>
    /// Represents the information pertaining to an inline binary resource
    /// </summary>
    internal class Pointer
    {
        /// <summary>
        /// The header collection that contains the pointer header
        /// </summary>
        private HeaderCollection headerCollection;

        /// <summary>
        /// The identifier value
        /// </summary>
        private string identifier;

        /// <summary>
        /// The length of the data
        /// </summary>
        private int length = 0;

        /// <summary>
        /// The actual data
        /// </summary>
        private byte[] byteArray;

        /// <summary>
        /// Creates a new instance of the <see cref="Pointer"/> class
        /// </summary>
        /// <param name="headerCollection">The <see cref="HeaderCollection"/> that contains the pointer header</param>
        public Pointer(HeaderCollection headerCollection)
        {
            this.headerCollection = headerCollection;
        }

        /// <summary>
        /// Gets the <see cref="HeaderCollection"/> that contains the pointer header
        /// </summary>
        /// <value>
        /// <see cref="HeaderCollection"/>
        /// </value>
        public HeaderCollection HeaderCollection
        {
            get
            {
                return this.headerCollection;
            }
        }

        /// <summary>
        /// The identifier value of the resource
        /// </summary>
        /// <value>
        /// string
        /// </value>
        public string Identifier
        {
            get
            {
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        /// <summary>
        /// The length of the data
        /// </summary>
        /// <value>
        /// int
        /// </value>
        public int Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
            }
        }

        /// <summary>
        /// The actual data
        /// </summary>
        /// <value>
        /// array of bytes
        /// </value>
        public byte[] ByteArray
        {
            get
            {
                return this.byteArray;
            }
            set
            {
                this.byteArray = value;
                this.Length = value.Length;
                this.HeaderCollection.AssociateBinaryData(new BinaryData(this.Identifier, value));
            }
        }

        /// <summary>
        /// Clears the pointer of any resource information.
        /// </summary>
        /// <remarks>
        /// This is usually used if we read some data from the cache, but we need to read the data from the request instead.
        /// </remarks>
        internal void Clear()
        {
            this.identifier = null;
            this.byteArray = null;
            this.length = 0;
        }
    }
}
