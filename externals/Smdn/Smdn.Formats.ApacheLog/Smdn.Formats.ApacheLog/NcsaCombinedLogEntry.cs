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
using System.Net;

using Smdn.Formats.ApacheLog.Entities;

namespace Smdn.Formats.ApacheLog {
  [ApacheLogFormat("%h %l %u %t \"%r\" %>s %b \"%{Referer}i\" \"%{User-agent}i\"")]
  public class NcsaCombinedLogEntry :
    LogEntry,
    IRemoteLogName,
    IRemoteHost,
    IRemoteUser,
    IResponseLengthCLF,
    IRequestedTime,
    IRequestHeaders,
    IRequestLine,
    IStatusCode
  {
    internal static string LogFormatString = "%h %l %u %t \"%r\" %>s %b \"%{Referer}i\" \"%{User-agent}i\"";

    public NcsaCombinedLogEntry()
    {
    }

    public string RemoteLogName {
      get; set;
    }

    public string RemoteHost {
      get; set;
    }

    public string RemoteUser {
      get; set;
    }

    public long? ResponseLengthCLF {
      get; set;
    }

    public DateTimeOffset RequestedTime {
      get; set;
    }

    public WebHeaderCollection RequestHeaders {
      get; set;
    }

    public string RequestLine {
      get; set;
    }

    public HttpStatusCode StatusCode {
      get; set;
    }

    public string UserAgent {
      get
      {
        if (RequestHeaders == null)
          return null;
        else
          return RequestHeaders[HttpRequestHeader.UserAgent];
      }
    }

    public string Referer {
      get
      {
        if (RequestHeaders == null)
          return null;
        else
          return RequestHeaders[HttpRequestHeader.Referer];
      }
    }

    public override string ToString()
    {
      return string.Format("{0} {1} {2} [{3}] \"{4}\" {5} {6} \"{7}\" \"{8}\"",
                           RemoteHost,
                           RemoteLogName ?? LogEntry.EmptyString,
                           RemoteUser ?? LogEntry.EmptyString,
                           RequestedTime.ToString("d/MMM/yyyy:hh:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture),
                           RequestLine,
                           (int)StatusCode,
                           ResponseLengthCLF,
                           Referer ?? LogEntry.EmptyString,
                           UserAgent ?? LogEntry.EmptyString);
    }
  }
}
