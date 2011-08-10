using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents a set of <see cref="IHeader"/>s
    /// </summary>
    public class HeaderCollection : List<IHeader>
    {
        /// <summary>
        /// Contains a list of just the regular (defined) headers in the collection
        /// </summary>
        private List<IHeader> headers = new List<IHeader>();

        /// <summary>
        /// Contains a list of just the custom headers in the collection
        /// </summary>
        private List<IHeader> customHeaders = new List<IHeader>();

        /// <summary>
        /// Contains a list of just the application-specific data headers in the collection
        /// </summary>
        private List<IHeader> dataHeaders = new List<IHeader>();

        /// <summary>
        /// Contains a list of just the resource pointer headers in the collection
        /// </summary>
        private List<IHeader> pointers = new List<IHeader>();

        /// <summary>
        /// Contains a list of all of the headers in the collection, regardless of type
        /// </summary>
        private Dictionary<string, IHeader> allHeaders = new Dictionary<string, IHeader>();

        /// <summary>
        /// Adds a <see cref="IHeader"/> to the collection
        /// </summary>
        /// <param name="header"><see cref="IHeader"/></param>
        public void AddHeader(IHeader header)
        {
            if (header != null && header.IsValid) {
                if (header.IsGrowlResourcePointer)
                    this.pointers.Add(header);
                else if (header.IsCustomHeader)
                    this.customHeaders.Add(header);
                else if (header.IsDataHeader)
                    this.dataHeaders.Add(header);
                else
                    this.headers.Add(header);
                this.Add(header);
                this.allHeaders.Add(header.Name, header);
            }
        }

        /// <summary>
        /// Adds all of the headers in <paramref name="headers"/> to the
        /// currently collection.
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> containing the headers to add</param>
        public void AddHeaders(HeaderCollection headers)
        {
            foreach (IHeader header in headers) {
                this.AddHeader(header);
            }
        }

        /// <summary>
        /// Gets a list of all of the normal (defined) headers in the collection, 
        /// excluding any custom headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<IHeader> Headers
        {
            get
            {
                return this.headers;
            }
        }

        /// <summary>
        /// Gets a list of all of the custom headers in the collection, 
        /// excluding any normal (defined) headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<IHeader> CustomHeaders
        {
            get
            {
                return this.customHeaders;
            }
        }

        /// <summary>
        /// Gets a list of all of the application-specific data headers in the collection, 
        /// excluding any normal (defined) headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<IHeader> DataHeaders
        {
            get
            {
                return this.dataHeaders;
            }
        }

        /// <summary>
        /// Gets a list of all of the resource pointer headers in the collection, 
        /// excluding any other headers.
        /// </summary>
        /// <value>
        /// <see cref="List{Header}"/>
        /// </value>
        public List<IHeader> Pointers
        {
            get
            {
                return this.pointers;
            }
        }

        /// <summary>
        /// Associates the specified <paramref name="binaryData"/> to its related header.
        /// </summary>
        /// <param name="binaryData"><see cref="BinaryData"/></param>
        public void AssociateBinaryData(BinaryData binaryData)
        {
            foreach (var header in this.pointers) {
                if (header.IsGrowlResourcePointer && header.GrowlResourcePointerID == binaryData.ID) {
                    header.GrowlResource = binaryData;
                    break;
                }
            }
        }

        /// <summary>
        /// Looks up the <see cref="IHeader"/> in the collection by the header name.
        /// </summary>
        /// <param name="name">The header name</param>
        /// <returns><see cref="IHeader"/></returns>
        public IHeader Get(string name)
        {
            if (this.allHeaders.ContainsKey(name)) {
                return this.allHeaders[name];
            }
            else {
                return Header.NotFoundHeader;
            }
        }

        /// <summary>
        /// Gets the string value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>string - header value</returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>null</c> will be returned.
        /// </remarks>
        public string GetHeaderStringValue(string name, bool required)
        {
            var header = Get(name);
            if (required && (header == null || header.Value == null)) ThrowRequiredHeaderMissingException(name);
            return header.Value;
        }

        /// <summary>
        /// Gets the string value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>string - header value</returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>null</c> will be returned.
        /// </remarks>
        public DateTimeOffset GetHeaderDateTimeOffsetValue(string name, bool required)
        {
            var text = GetHeaderStringValue(name, required);
            DateTimeOffset rc = DateTimeOffset.MinValue;
            if (!DateTimeOffset.TryParse(text, out rc) && required) ThrowRequiredHeaderMissingException(name);
            return rc;
        }

        /// <summary>
        /// Gets the boolean value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>bool - header value</returns>
        /// <remarks>
        /// Valid <c>true</c> values include "TRUE" and "YES" in upper or lower case - 
        /// all other values will be considered <c>false</c>.
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>false</c> will be returned.
        /// </remarks>
        public bool GetHeaderBooleanValue(string name, bool required)
        {
            var b = false;
            var val = GetHeaderStringValue(name, required);
            if (!String.IsNullOrEmpty(val)) {
                val = val.ToUpper();
                switch (val) {
                    case "TRUE":
                    case "YES":
                        b = true;
                        break;
                }
            }
            return b;
        }

        /// <summary>
        /// Gets the integer value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>int - header value</returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>zero</c> will be returned.
        /// </remarks>
        public int GetHeaderIntValue(string name, bool required)
        {
            var val = GetHeaderStringValue(name, required);
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// Gets the integer value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns>int - header value</returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>zero</c> will be returned.
        /// </remarks>
        public IntPtr GetHeaderIntPtrValue(string name, bool required)
        {
            var val = GetHeaderStringValue(name, required);
            var v64 = Convert.ToInt64(val);
            return new IntPtr(v64);
        }

        /// <summary>
        /// Gets the <see cref="Resource"/> value of a header based on the header name
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="required">Indicates if the header is a required header</param>
        /// <returns><see cref="Resource"/></returns>
        /// <remarks>
        /// If <paramref name="required"/> is <c>true</c> and the header is not found in the collection, 
        /// a <see cref="GrowlException"/> will be thrown. If the header is not required
        /// and not found, <c>null</c> will be returned.
        /// </remarks>
        public Resource GetHeaderResourceValue(string name, bool required)
        {
            var header = Get(name);
            if (required && (header == null || header.Value == null)) ThrowRequiredHeaderMissingException(name);
            if (header.IsGrowlResourcePointer)
                return header.GrowlResource;
            else
                return header.Value;
        }

        /// <summary>
        /// Creates a <see cref="HeaderCollection"/> from a message
        /// </summary>
        /// <param name="message">The message to parse</param>
        /// <returns><see cref="HeaderCollection"/></returns>
        public static HeaderCollection FromMessage(string message)
        {
            var headers = new HeaderCollection();
            var lines = message.Split('\r', '\n');
            foreach (var line in lines) {
                var header = Header.ParseHeader(line);
                if (header != null) {
                    headers.AddHeader(header);
                }
            }

            return headers;
        }

        /// <summary>
        /// Throws a <see cref="GrowlException"/> with an error description that indicates that
        /// a requested required header was not found.
        /// </summary>
        /// <param name="headerName">The header name that was not found</param>
        private static void ThrowRequiredHeaderMissingException(string headerName)
        {
            throw new GrowlException(ErrorCode.REQUIRED_HEADER_MISSING, ErrorDescription.REQUIRED_HEADER_MISSING, headerName);
        }
    }
}