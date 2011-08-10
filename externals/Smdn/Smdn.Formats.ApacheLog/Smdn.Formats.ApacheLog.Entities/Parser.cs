// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Net;
using System.Globalization;

using Smdn.Formats.ApacheLog.FormatExpressions;

namespace Smdn.Formats.ApacheLog.Entities {
  internal static class Parser {
    public static TLogEntry ParseLine<TLogEntry>(FormatExpression[] expressions, string line) where TLogEntry : LogEntry, new()
    {
      var logEntry = new TLogEntry();

      ParseLine(logEntry, expressions, line);

      return logEntry;
    }

    private static void ParseLine(LogEntry logEntry, FormatExpression[] expressions, string line)
    {
      var chars = line.ToCharArray();
      var index = 0;
      var expressionIndex = 0;

      // first delimiter
      if (expressions[expressionIndex] is DelimiterFormatExpression) {
        var delimiter = (expressions[expressionIndex] as DelimiterFormatExpression).Delimiter;
        var delimiterIndex = 0;

        for (;;) {
          if (index == chars.Length)
            throw new FormatException();
          else if (delimiterIndex == delimiter.Length)
            break;
          else if (chars[index++] != delimiter[delimiterIndex++])
            throw new FormatException();
        }

        expressionIndex++;
      }

      for (;;) {
        if (index == chars.Length)
          break;

        else {
          var entity = expressions[expressionIndex] as EntityFormatExpression;
          var delimiter = (expressions[expressionIndex + 1] as DelimiterFormatExpression).Delimiter; // XXX
          var delimiterIndex = 0;
          var entityValueStartIndex = index;
          var entityValueLength = 0;

          for (;;) {
            var delimFound = false;

            if (index == chars.Length) {
              if (expressionIndex + 2 == expressions.Length)
                delimFound = true;
              else
                throw new FormatException(string.Format("unexpected end-of-line. {0}: \"{1}\"", Environment.NewLine, line));
            }
            else if (delimiterIndex == delimiter.Length) {
              delimFound = true;
            }
            else if (chars[index++] != delimiter[delimiterIndex++]) {
              delimiterIndex = 0;
            }

            if (delimFound) {
              entityValueLength = index - entityValueStartIndex - delimiter.Length;
              break;
            }
          }

          var entityValue = new string(chars, entityValueStartIndex, entityValueLength);

          switch (entity.Entity) {
            case "a":   if (logEntry is IRemoteIPAddress)       Parse(logEntry as IRemoteIPAddress, entityValue); break; // Remote IP-address
            case "A":   /*if (logEntry is ILocalIPAddress)*/ break; // Local IP-address
            case "B":   /*if (logEntry is IResponseLength)*/ break; // Size of response in bytes, excluding HTTP headers.
            case "b":   if (logEntry is IResponseLengthCLF)     Parse(logEntry as IResponseLengthCLF, entityValue); break; // Size of response in bytes, excluding HTTP headers. In CLF format, i.e. a '-' rather than a 0 when no bytes are sent.
            case "{}C": /*if (logEntry is ICookie)*/ break; // The contents of cookie Foobar in the request sent to the server.
            case "D":   /*if (logEntry is IServeRequestTakenTime)*/ break; // The time taken to serve the request, in microseconds.
            case "{}e": /*if (logEntry is IEnvironmentVariable)*/ break; // The contents of the environment variable FOOBAR
            case "f":   /*if (logEntry is IFileName)*/ break; // Filename
            case "h":   if (logEntry is IRemoteHost)            Parse(logEntry as IRemoteHost, entityValue); break; // Remote host
            case "H":   /*if (logEntry is IRequestProtocol)*/ break; // The request protocol
            case "{}i": if (logEntry is IRequestHeaders)        Parse(logEntry as IRequestHeaders, entity.Key, entityValue); break; // The contents of Foobar: header line(s) in the request sent to the server. Changes made by other modules (e.g. mod_headers) affect this.
            case "k":   /*if (logEntry is INumberOfKeeyAlive)*/ break; // Number of keepalive requests handled on this connection. Interesting if KeepAlive is being used, so that, for example, a '1' means the first keepalive request after the initial one, '2' the second, etc...; otherwise this is always 0 (indicating the initial request).
            case "l":   if (logEntry is IRemoteLogName)         Parse(logEntry as IRemoteLogName, entityValue); break; // Remote logname (from identd, if supplied). This will return a dash unless mod_ident is present and IdentityCheck is set On.
            case "m":   /*if (logEntry is IRequestMethod)*/ break; // The request method
            case "{}n": /*if (logEntry is INotes)*/ break; // The contents of note Foobar from another module.
            case "{}o": if (logEntry is IResponseHeaders)       Parse(logEntry as IResponseHeaders, entity.Key, entityValue); break; // The contents of Foobar: header line(s) in the reply.
            case "p":   /*if (logEntry is IXxx)*/ break; // The canonical port of the server serving the request
            case "{}p": /*if (logEntry is IXxx)*/ break; // The canonical port of the server serving the request or the server's actual port or the client's actual port. Valid formats are canonical, local, or remote.
            case "P":   /*if (logEntry is IXxx)*/ break; // The process ID of the child that serviced the request.
            case "{}P": /*if (logEntry is IXxx)*/ break; // The process ID or thread id of the child that serviced the request. Valid formats are pid, tid, and hextid. hextid requires APR 1.2.0 or higher.
            case "q":   /*if (logEntry is IQueryString)*/ break; // The query string (prepended with a ? if a query string exists, otherwise an empty string)
            case "r":   if (logEntry is IRequestLine)           Parse(logEntry as IRequestLine, entityValue); break; // First line of request
            case "s":   if (logEntry is IStatusCode)            Parse(logEntry as IStatusCode, entityValue); break; // Status. For requests that got internally redirected, this is the status of the *original* request --- %>s for the last.
            case "t":   if (logEntry is IRequestedTime)         Parse(logEntry as IRequestedTime, entityValue); break; // Time the request was received (standard english format)
            case "{}t": if (logEntry is IRequestedTime)         Parse(logEntry as IRequestedTime, entity.Key, entityValue); break; // The time, in the form given by format, which should be in strftime(3) format. (potentially localized)
            case "T":   /*if (logEntry is IServeRequestTakenTime)*/ break; // The time taken to serve the request, in seconds.
            case "u":   if (logEntry is IRemoteUser)            Parse(logEntry as IRemoteUser, entityValue); break;
            case "U":   /*if (logEntry is IRequestedPath)*/ break; // The URL path requested, not including any query string.
            case "v":   /*if (logEntry is IServerName)*/ break; // The canonical ServerName of the server serving the request.
            case "V":   /*if (logEntry is ICanonicalServerName)*/ break; // The server name according to the UseCanonicalName setting.
            case "X":   /*if (logEntry is IConnectionStatus)*/ break; // Connection status when response is completed:
            case "I":   /*if (logEntry is IReceivedByteCount)*/ break; // Bytes received, including request and headers, cannot be zero. You need to enable mod_logio to use this.
            case "O":   /*if (logEntry is ISentByteCount)*/ break; // Bytes sent, including headers, cannot be zero. You need to enable mod_logio to use this.
            default:    break;
          }

          expressionIndex += 2;
        }
      }
    }

    public static void Parse(IRemoteIPAddress logEntry, string entity)
    {
      logEntry.RemoteIPAddress = IPAddress.Parse(entity);
    }

    public static void Parse(IRemoteHost logEntry, string entity)
    {
      logEntry.RemoteHost = entity;
    }

    public static void Parse(IRemoteLogName logEntry, string entity)
    {
      logEntry.RemoteLogName = entity;
    }

    public static void Parse(IRemoteUser logEntry, string entity)
    {
      logEntry.RemoteUser = (entity == LogEntry.EmptyString) ? null : entity;
    }

    public static void Parse(IResponseLengthCLF logEntry, string entity)
    {
      if (entity == LogEntry.EmptyString)
        logEntry.ResponseLengthCLF = null;
      else
        logEntry.ResponseLengthCLF = long.Parse(entity);
    }

    public static void Parse(IStatusCode logEntry, string entity)
    {
      logEntry.StatusCode = (HttpStatusCode)int.Parse(entity);
    }

    public static void Parse(IRequestedTime logEntry, string entity)
    {
      entity = entity.Substring(1, entity.Length - 2); // remove brackets

      // MONO-BUG
      logEntry.RequestedTime = DateTimeOffset.ParseExact(entity, new[]{"d/MMM/yyyy:HH:mm:ss zz", "d/MMM/yyyy:HH:mm:ss zzz"}, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }

    public static void Parse(IRequestedTime logEntry, string key, string entity)
    {
      entity = entity.Substring(1, entity.Length - 2); // remove brackets

      logEntry.RequestedTime = DateTimeOffset.ParseExact(entity, key, CultureInfo.InvariantCulture); // TODO: parse format
    }

    public static void Parse(IRequestHeaders logEntry, string key, string entity)
    {
      if (entity == LogEntry.EmptyString)
        return;

      if (logEntry.RequestHeaders == null)
        logEntry.RequestHeaders = new WebHeaderCollection();

      logEntry.RequestHeaders.Add(key, entity);
    }

    public static void Parse(IRequestLine logEntry, string entity)
    {
      logEntry.RequestLine = entity;
    }

    public static void Parse(IResponseHeaders logEntry, string key, string entity)
    {
      if (entity == LogEntry.EmptyString)
        return;

      if (logEntry.ResponseHeaders == null)
        logEntry.ResponseHeaders = new WebHeaderCollection();

      logEntry.ResponseHeaders.Add(key, entity);
    }
  }
}
