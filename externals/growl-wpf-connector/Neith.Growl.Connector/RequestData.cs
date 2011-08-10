using System;
using System.Collections.Generic;
using System.Text;
using Neith.Growl.CoreLibrary;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Represents additional application-specific data that can be passed with a request and
    /// will be returned with the response from Growl. The actual items and their values are
    /// not used by Growl.
    /// </summary>
    [Serializable]
    public class RequestData : Dictionary<string, string>
    {

        /// <summary>
        /// Creates a new <see cref="RequestData"/> from a list of headers
        /// </summary>
        /// <param name="headers">The <see cref="HeaderCollection"/> used to populate the object</param>
        /// <returns><see cref="RequestData"/></returns>
        public static RequestData FromHeaders(HeaderCollection headers)
        {
            var rd = new RequestData();

            if (headers != null)
            {
                foreach (var header in headers.DataHeaders)
                {
                    if (header != null)
                    {
                        rd.Add(header.ActualName, header.Value);
                    }
                }
            }

            return rd;
        }
    }
}
