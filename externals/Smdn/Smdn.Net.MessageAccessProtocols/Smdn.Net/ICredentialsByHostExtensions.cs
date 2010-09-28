// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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

using Smdn.Net.MessageAccessProtocols;

namespace Smdn.Net {
  public static class ICredentialsByHostExtensions {
    public static NetworkCredential LookupCredential(this ICredentialsByHost credentials, ConnectionBase connection, string username, IStringEnum authenticationMechanism)
    {
      if (connection == null)
        throw new ArgumentNullException("connection");

      return LookupCredential(credentials,
                              connection.Host,
                              connection.Port,
                              username,
                              authenticationMechanism == null
                                ? null
                                : authenticationMechanism.Value);
    }

    public static NetworkCredential LookupCredential(this ICredentialsByHost credentials, string host, int port, string username, string authenticationMechanism)
    {
      if (credentials == null)
        return null;
      if (host == null)
        throw new ArgumentNullException("host");

      var credential = credentials.GetCredential(host, port, (authenticationMechanism == null) ? string.Empty : authenticationMechanism);

      if (credential == null || (!string.IsNullOrEmpty(username) && credential.UserName != username))
        return null;
      else
        return credential;
    }
  }
}
