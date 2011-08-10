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
using System.Text;

using Smdn.Formats;
using Smdn.Net.Pop3.Protocol;

namespace Smdn.Net.Pop3 {
  public class PopStyleUriParser : GenericUriParser {
    /*
     * class members
     */

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

    /// <summary>returns [enc-user "@"] host ":" port</summary>
    public static string GetAuthority(Uri uri)
    {
      return GetAuthority(uri, false);
    }

    /// <summary>returns [user-auth "@"] host ":" port</summary>
    public static string GetStrongAuthority(Uri uri)
    {
      return GetAuthority(uri, true);
    }

    private static string GetAuthority(Uri uri, bool strong)
    {
      CheckUriScheme(uri);

      var userinfo = strong ? uri.UserInfo : GetUser(uri);

      if (userinfo.Length == 0)
        return string.Concat(uri.Host, ":", GetPort(uri));
      else
        return string.Concat(userinfo, "@", uri.Host, ":", GetPort(uri));
    }

    private static int GetPort(Uri uri)
    {
      if (uri.Port == -1)
        return PopUri.GetDefaultPortFromScheme(uri);
      else
        return uri.Port;
    }

    private static string GetEncUser(Uri uri)
    {
      CheckUriScheme(uri);

      var userInfo = uri.UserInfo.Split(':');

      if (userInfo.Length == 0)
        return string.Empty;
      else
        return userInfo[0];
    }

    /// <summary>returns user (unescaped enc-user)</summary>
    public static string GetUser(Uri uri)
    {
      var encUser = GetEncUser(uri);
      var index = encUser.IndexOf(";AUTH=", StringComparison.OrdinalIgnoreCase);

      return Uri.UnescapeDataString((index < 0) ? encUser : encUser.Substring(0, index));
    }

    /// <summary>returns authentication type (unescaped enc-auth-type)</summary>
    public static PopAuthenticationMechanism GetAuthType(Uri uri)
    {
      /*
       *         auth             = ";AUTH=" ( "*" / enc-auth-type )
       *         enc-auth-type    = enc-sasl / enc-ext
       *         enc-ext          = "+" ("APOP" / 1*achar)
       *                               ;APOP or encoded extension mechanism name
       *         enc-sasl         = 1*achar
       *                               ;encoded version of [SASL] "auth_type"
       */
      var encUser = GetEncUser(uri);
      var index = encUser.IndexOf(";AUTH=", StringComparison.OrdinalIgnoreCase);

      if (index < 0)
        return null;

      var authType = encUser.Substring(index + 6);

      if (authType.StartsWith("%2b", StringComparison.OrdinalIgnoreCase)) // '+'
        authType = authType.Substring(0, 3) + Uri.UnescapeDataString(authType.Substring(3));
      else if (!string.Equals(authType, "%2a", StringComparison.OrdinalIgnoreCase)) // '*'
        authType = Uri.UnescapeDataString(authType);

      return PopAuthenticationMechanism.GetKnownOrCreate(authType);
    }

    protected static void CheckUriScheme(Uri uri)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      if (PopUri.IsPop(uri))
        return;
      else
        throw new ArgumentException("uri is not POP URL", "uri");
    }

    /*
     * instance members
     */
    private static readonly GenericUriParserOptions parserOptions =
      GenericUriParserOptions.AllowEmptyAuthority |
      GenericUriParserOptions.DontCompressPath |
      GenericUriParserOptions.DontConvertPathBackslashes |
      GenericUriParserOptions.DontUnescapePathDotsAndSlashes |
      GenericUriParserOptions.GenericAuthority;

    public PopStyleUriParser()
      : base(parserOptions)
    {
    }

    protected override void InitializeAndValidate(Uri uri, out UriFormatException parsingError)
    {
      base.InitializeAndValidate(uri, out parsingError);
    }
  }
}
