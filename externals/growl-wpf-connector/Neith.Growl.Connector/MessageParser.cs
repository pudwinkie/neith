using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Neith.Growl.Connector
{
    /// <summary>
    /// Provides the ability to parse a response message
    /// </summary>
    public class MessageParser
    {
        /// <summary>
        /// The GNTP protocol version supported by this parser
        /// </summary>
        public const string GNTP_SUPPORTED_VERSION = "1.0";

        /// <summary>
        /// A string representing a line ending (Carriage Return + Line Feed)
        /// </summary>
        public const string BLANK_LINE = "\r\n";

        /// <summary>
        /// Regular expression for parsing the message header
        /// </summary>
        private static Regex regExMessageHeader = new Regex(@"(GNTP/)(?<Version>(.\..))\s+(?<Directive>(\S+))");

        /// <summary>
        /// The protocol version of the response message
        /// </summary>
        private string version;

        /// <summary>
        /// The response directive
        /// </summary>
        private string directive;

        /// <summary>
        /// Parses a response message and returns the corresponding <see cref="IResponse"/> object
        /// </summary>
        /// <param name="message">The entire GNTP response message</param>
        /// <param name="context">If this is a CALLBACK response, returns the <see cref="ICallbackData"/> associated with the response; otherwise <c>null</c></param>
        /// <returns><see cref="IResponse"/>The <see cref="IResponse"/> represented by the message</returns>
        public IResponse Parse(string message, out ICallbackData context)
        {
            HeaderCollection headers;
            return Parse(message, out context, out headers);
        }

        /// <summary>
        /// Parses a response message and returns the corresponding <see cref="IResponse"/> object, returning the list of parsed headers as well.
        /// </summary>
        /// <param name="message">The entire GNTP response message</param>
        /// <param name="context">If this is a CALLBACK response, returns the <see cref="ICallbackData"/> associated with the response; otherwise <c>null</c></param>
        /// <param name="headers">Contains the list of parsed headers</param>
        /// <returns><see cref="IResponse"/>The <see cref="IResponse"/> represented by the message</returns>
        public IResponse Parse(string message, out ICallbackData context, out HeaderCollection headers)
        {
            context = null;
            headers = null;
            var response = Parse(message, out headers);
            context = response.CallbackData;

            return response;
        }

        /// <summary>
        /// Parses a response message and returns the corresponding <see cref="IResponse"/> object
        /// </summary>
        /// <param name="message">The entire GNTP response message</param>
        /// <param name="headers">The <see cref="HeaderCollection"/> of parsed header values</param>
        /// <returns><see cref="IResponse"/></returns>
        private IResponse Parse(string message, out HeaderCollection headers)
        {
            var responseType = ResponseType.ERROR;
            IResponse response = null;
            headers = new HeaderCollection();

            var bytes = System.Text.Encoding.UTF8.GetBytes(message);
            using (var stream = new System.IO.MemoryStream(bytes))
            using (var reader = new System.IO.StreamReader(stream)) {
                var isError = false;
                var isFirstLine = true;
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();

                    if (isFirstLine) {
                        var match = ParseGNTPHeaderLine(line);
                        if (match.Success) {
                            this.version = match.Groups["Version"].Value;
                            this.directive = match.Groups["Directive"].Value;
                            if (this.directive.StartsWith("-", StringComparison.InvariantCulture)) this.directive = this.directive.Remove(0, 1);

                            if (version == GNTP_SUPPORTED_VERSION) {
                                if (Enum.IsDefined(typeof(ResponseType), this.directive)) {
                                    responseType = (ResponseType)Enum.Parse(typeof(ResponseType), this.directive, false);
                                    response = new Response();
                                    if (responseType == ResponseType.ERROR) isError = true;
                                    isFirstLine = false;
                                }
                                else {
                                    // invalid directive
                                    response = new Response(ErrorCode.INVALID_REQUEST, "Unrecognized response type");
                                    break;
                                }
                            }
                            else {
                                // invalid version
                                response = new Response(ErrorCode.UNKNOWN_PROTOCOL_VERSION, "Unsupported version");
                                break;
                            }
                        }
                        else {
                            // invalid message header
                            response = new Response(ErrorCode.UNKNOWN_PROTOCOL, "Unrecognized response");
                            break;
                        }
                    }
                    else {
                        var header = Header.ParseHeader(line);
                        headers.AddHeader(header);
                    }
                }

                if (response != null) {
                    if (isError) {
                        var errorCode = headers.GetHeaderIntValue(HeaderKeys.ERROR_CODE, false);
                        var errorDescription = headers.GetHeaderStringValue(HeaderKeys.ERROR_DESCRIPTION, false);
                        if (errorCode > 0 && errorDescription != null)
                            response = new Response(errorCode, errorDescription);
                        else
                            response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
                    }
                    else {
                        var inResponseTo = headers.GetHeaderStringValue(HeaderKeys.RESPONSE_ACTION, false);
                        response.InResponseTo = inResponseTo;
                    }

                    response.SetAttributesFromHeaders(headers, (responseType == ResponseType.CALLBACK));
                }
                else {
                    // if we got here, that is bad.
                    response = new Response(ErrorCode.INTERNAL_SERVER_ERROR, ErrorDescription.INTERNAL_SERVER_ERROR);
                }
            }


            return response;
        }

        /// <summary>
        /// Parses a GNTP header line and returns the RegEx matches
        /// </summary>
        /// <param name="line">The GNTP header line not including the ending line breaks</param>
        /// <returns>RegEx <see cref="Match"/></returns>
        public static Match ParseGNTPHeaderLine(string line)
        {
            return regExMessageHeader.Match(line);
        }
    }
}
