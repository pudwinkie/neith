// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2008-2011 smdn
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
using System.Text;

using Smdn.Formats;

namespace Smdn.Net.Pop3 {
  public class PopUriBuilder :
    IEquatable<PopUriBuilder>,
    IEquatable<Uri>,
    ICloneable
  {
    /// <value>The scheme of the POP URL.</value>
    public string Scheme {
      get { return scheme; }
      set
      {
        if (value == null)
          scheme = PopUri.UriSchemePop;

        if (!string.Equals(value, PopUri.UriSchemePop, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(value, PopUri.UriSchemePops, StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException(string.Format("scheme must be '{0}' or '{1}'", PopUri.UriSchemePop, PopUri.UriSchemePops), "Scheme");

        scheme = value;
        uri = null;
      }
    }

    /// <value>The host name of the POP URL.</value>
    public string Host {
      get { return host; }
      set { host = value ?? string.Empty; uri = null; }
    }

    /// <value>The port number of the POP URL.</value>
    /// <remarks>If a port is not specified as part of the POP URL, the <see cref="Port"/> property returns value -1.</remarks>
    public int Port {
      get { return port; }
      set
      {
        if (value < -1)
          throw ExceptionUtils.CreateArgumentMustBeGreaterThanOrEqualTo(-1, "Port", value);

        port = value;
        uri = null;
      }
    }

    /// <value>The user name of the user that accesses the POP URL. </value>
    public string UserName {
      get { return userName; }
      set { userName = value ?? string.Empty; uri = null; }
    }

    /// <value>The authentication mechanism that is used to access the POP URL.</value>
    public PopAuthenticationMechanism AuthType {
      get { return authType; }
      set { authType = value; uri = null; }
    }

    /// <value>A <see cref="System.Uri"/> that contains the URI constructed by the <see cref="PopUriBuilder"/>.</value>
    public Uri Uri {
      get
      {
        if (uri == null)
          uri = new Uri(ToString());

        return uri;
      }
    }

    public PopUriBuilder(string uri)
      : this(new Uri(uri))
    {
    }

    public PopUriBuilder(Uri uri)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      Scheme = uri.Scheme;
      Host = uri.Host;
      Port = uri.Port;
      UserName = PopStyleUriParser.GetUser(uri);
      AuthType = PopStyleUriParser.GetAuthType(uri);
    }

    public PopUriBuilder()
      : this(false, "localhost", -1, null, null)
    {
    }

    public PopUriBuilder(bool popsScheme, string host)
      : this(popsScheme, host, -1, null, null)
    {
    }

    public PopUriBuilder(string host, int port)
      : this(false, host, port, null, null)
    {
    }

    public PopUriBuilder(bool popsScheme, string host, int port)
      : this(popsScheme, host, port, null, null)
    {
    }

    public PopUriBuilder(string host, string userName, PopAuthenticationMechanism authType)
      : this(false, host, -1, userName, authType)
    {
    }

    public PopUriBuilder(bool popsScheme, string host, string userName, PopAuthenticationMechanism authType)
      : this(popsScheme, host, -1, userName, authType)
    {
    }

    public PopUriBuilder(string host, int port, string userName, PopAuthenticationMechanism authType)
      : this(false, host, port, userName, authType)
    {
    }

    public PopUriBuilder(bool popsScheme, string host, int port, string userName, PopAuthenticationMechanism authType)
    {
      Scheme = popsScheme ? PopUri.UriSchemePops : PopUri.UriSchemePop;
      Host = host;
      Port = port;
      UserName = userName;
      AuthType = authType;
    }

#region "equality"
    public override bool Equals(object obj)
    {
      var builder = obj as PopUriBuilder;

      if (builder == null) {
        var uri = obj as Uri;

        if (uri == null)
          return false;
        else
          return Equals(uri);
      }
      else {
        return Equals(builder);
      }
    }

    public bool Equals(PopUriBuilder other)
    {
      if (other == null)
        return false;
      else
        return Equals(other.Uri);
    }

    public bool Equals(Uri other)
    {
      return this.Uri.Equals(other);
    }

    public override int GetHashCode()
    {
      return ToString().GetHashCode();
    }
#endregion

    public PopUriBuilder Clone()
    {
      var cloned = MemberwiseClone() as PopUriBuilder;

      if (this.authType != null)
        cloned.authType = PopAuthenticationMechanism.GetKnownOrCreate((string)this.authType);

      return cloned;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    public override string ToString()
    {
      /*
       * RFC 2384 POP URL Scheme
       * http://tools.ietf.org/html/rfc2384
       *    A POP URL is of the general form:
       * 
       *         pop://<user>;auth=<auth>@<host>:<port>
       * 
       *    Where <user>, <host>, and <port> are as defined in RFC 1738, and some
       *    or all of the elements, except "pop://" and <host>, may be omitted.
       * 
       * http://tools.ietf.org/html/rfc3986
       * RFC 3986 - Uniform Resource Identifier (URI): Generic Syntax
       */
      var sb = new StringBuilder(512);

      sb.Append(scheme);
      sb.Append(Uri.SchemeDelimiter);

      /*
       * 8. ABNF for POP URL scheme
       *    The POP URL scheme is described using [ABNF]:
       *         achar            = uchar / "&" / "=" / "~"
       *                                 ; see [BASIC-URL] for "uchar" definition
       *         auth             = ";AUTH=" ( "*" / enc-auth-type )
       *         enc-auth-type    = enc-sasl / enc-ext
       *         enc-ext          = "+" ("APOP" / 1*achar)
       *                               ;APOP or encoded extension mechanism name
       *         enc-sasl         = 1*achar
       *                               ;encoded version of [SASL] "auth_type"
       *         enc-user         = 1*achar
       *                               ;encoded version of [POP3] mailbox
       *         pop-url          = "pop://" server
       *         server           = [user-auth "@"] hostport
       *                               ;See [BASIC-URL] for "hostport" definition
       *         user-auth        = enc-user [auth]
       */
      sb.Append(PercentEncoding.GetEncodedString(userName,
                                                 ToPercentEncodedTransformMode.Rfc3986Uri));

      if (authType != null) {
        sb.Append(";AUTH=");

        if (authType == PopAuthenticationMechanism.SelectAppropriate)
          sb.Append(authType);
        else
          sb.Append(PercentEncoding.GetEncodedString((string)authType,
                                                     ToPercentEncodedTransformMode.Rfc3986Uri));
      }

      if (0 < userName.Length || authType != null)
        sb.Append('@');

      sb.Append(host);

      if (0 <= port) {
        sb.Append(':');
        sb.Append(port);
      }

      sb.Append('/');

      return sb.ToString();
    }

    private Uri uri;

    private string scheme = PopUri.UriSchemePop;
    private string host = string.Empty;
    private int port = -1;
    private string userName = string.Empty;
    private PopAuthenticationMechanism authType = null;
  }
}
