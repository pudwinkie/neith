// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2008-2010 smdn
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
using Smdn.Net.Imap4.Protocol;

namespace Smdn.Net.Imap4 {
  public class ImapStyleUriParser : GenericUriParser {
    /*
     * class members
     */

    /*
     *    iserver          = [iuserinfo "@"] host [ ":" port ]
     *                            ; This is the same as "authority" defined
     *                            ; in [URI-GEN].  See [URI-GEN] for "host"
     *                            ; and "port" definitions.
     */

    /// <summary>returns [enc-user "@"] host ":" port</summary>
    public static string GetAuthority(Uri uri)
    {
      return GetAuthority(uri, false);
    }

    /// <summary>returns [iuserinfo "@"] host ":" port</summary>
    public static string GetStrongAuthority(Uri uri)
    {
      return GetAuthority(uri, true);
    }

    private static string GetAuthority(Uri uri, bool strong)
    {
      CheckUriScheme(uri);

      var userinfo = strong ? uri.UserInfo : GetUser(uri);

      if (string.Empty.Equals(userinfo))
        return string.Concat(uri.Host, ":", GetPort(uri));
      else
        return string.Concat(userinfo, "@", uri.Host, ":", GetPort(uri));
    }

    private static int GetPort(Uri uri)
    {
      if (uri.Port == -1)
        return ImapUri.GetDefaultPortFromScheme(uri);
      else
        return uri.Port;
    }

    /*
     *    iuserinfo        = enc-user [iauth] / [enc-user] iauth
     *                                 ; conforms to the generic syntax of
     *                                 ; "userinfo" as defined in [URI-GEN].
     *    iauth            = ";AUTH=" ( "*" / enc-auth-type )
     */

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
    public static ImapAuthenticationMechanism GetAuthType(Uri uri)
    {
      var encUser = GetEncUser(uri);
      var index = encUser.IndexOf(";AUTH=", StringComparison.OrdinalIgnoreCase);

      if (index < 0)
        return null;

      /*
       *    The string ";AUTH=*" indicates that the client SHOULD select an
       *    appropriate authentication mechanism.  (Though the '*' character in
       *    this usage is not strictly a delimiter, it is being treated like a
       *    sub-delim [URI-GEN] in this instance.  It MUST NOT be percent-encoded
       *    in this usage, as ";AUTH=%2A" will not match this production.)
       */
      var authType = encUser.Substring(index + 6);

      if (!string.Equals(authType, "%2a", StringComparison.OrdinalIgnoreCase))
        authType = Uri.UnescapeDataString(authType);

      return ImapAuthenticationMechanism.GetKnownOrCreate(authType);
    }

    /*
     *    imailbox-ref     = enc-mailbox [uidvalidity]
     *    enc-mailbox      = 1*bchar
     *                   ; %-encoded version of [IMAP4] "mailbox"
     *    bchar            = achar / ":" / "@" / "/"
     *    achar            = uchar / "&" / "="
     *                       ;; Same as [URI-GEN] 'unreserved / sub-delims /
     *                       ;; pct-encoded', but ";" is disallowed.
     *    uchar            = unreserved / sub-delims-sh / pct-encoded
     *    sub-delims-sh = "!" / "$" / "'" / "(" / ")" /
     *                    "*" / "+" / ","
     *                       ;; Same as [URI-GEN] sub-delims,
     *                       ;; but without ";", "&" and "=".
     */

    /// <summary>returns mailbox (unescaped enc-mailbox)</summary>
    public static string GetMailbox(Uri uri)
    {
      /*
       *    If an IMAP server allows for mailbox names starting with "./" or
       *    "../", ending with "/." or "/..", or containing sequences "/../" or
       *    "/./", then such mailbox names MUST be percent-encoded as described
       *    in [URI-GEN].  Otherwise, they would be misinterpreted as dot-
       *    segments (see Section 3.3 of [URI-GEN]), which are processed
       *    specially during the relative path resolution process.
       */
      CheckUriScheme(uri);

      // regularize
      uri = new Uri(uri.AbsoluteUri);

      var path = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
      var delim = path.IndexOf(';');

      if (0 <= delim)
        path = path.Substring(0, delim);

      if (path.EndsWith("/", StringComparison.Ordinal))
        path = path.Substring(0, path.Length - 1);

      return Uri.UnescapeDataString(path);
    }

    /*
     *    uidvalidity      = ";UIDVALIDITY=" nz-number
     *                        ; See [IMAP4] for "nz-number" definition
     */

    /// <summary>returns UIDVALIDITY value</summary>
    public static long GetUidValidity(Uri uri)
    {
      CheckUriScheme(uri);

      var path = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
      var delim = path.IndexOf(";UIDVALIDITY=", StringComparison.OrdinalIgnoreCase);

      if (delim < 0)
        return 0;

      var uidvalidity = path.Substring(delim + 13);

      delim = uidvalidity.IndexOf('/');

      if (0 <= delim)
        uidvalidity = uidvalidity.Substring(0, delim);

      return ImapDataConverter.ToNonZeroNumber(uidvalidity);
    }

    /*
     *    iuid             = "/" iuid-only
     * 
     *    iuid-only        = ";UID=" nz-number
     *                   ; See [IMAP4] for "nz-number" definition
     */

    /// <summary>returns UID value</summary>
    public static long GetUid(Uri uri)
    {
      CheckUriScheme(uri);

      var segments = uri.Segments;

      for (var i = 2; i < segments.Length; i++) {
        if (segments[i].StartsWith(";UID=", StringComparison.OrdinalIgnoreCase)) {
          var uid = segments[i].Substring(5);

          if (uid.EndsWith("/", StringComparison.Ordinal))
            uid = uid.Substring(0, uid.Length - 1);

          return ImapDataConverter.ToNonZeroNumber(uid);
        }
      }

      return 0;
    }

    /*
     *    isection         = "/" isection-only
     * 
     *    isection-only    = ";SECTION=" enc-section
     * 
     *    enc-section      = 1*bchar
     *                   ; %-encoded version of [IMAP4] "section-spec"
     */

    /// <summary>returns SECTION value (unescaped enc-section)</summary>
    public static string GetSection(Uri uri)
    {
      CheckUriScheme(uri);

      var segments = uri.Segments;

      for (var i = 3; i < segments.Length; i++) {
        if (segments[i].StartsWith(";SECTION=", StringComparison.OrdinalIgnoreCase)) {
          var section = segments[i].Substring(9);

          return Uri.UnescapeDataString(section.EndsWith("/", StringComparison.Ordinal)
                                        ? section.Substring(0, section.Length - 1)
                                        : section);
        }
      }

      return string.Empty;
    }

    /*
     *    ipartial         = "/" ipartial-only
     * 
     *    ipartial-only    = ";PARTIAL=" partial-range
     * 
     *    partial-range    = number ["." nz-number]
     *                   ; partial FETCH.  The first number is
     *                            ; the offset of the first byte,
     *                            ; the second number is the length of
     *                            ; the fragment.
     */

    /// <summary>returns PARTIAL value</summary>
    public static ImapPartialRange? GetPartial(Uri uri)
    {
      CheckUriScheme(uri);

      var segments = uri.Segments;

      for (var i = 3; i < segments.Length; i++) {
        if (segments[i].StartsWith(";PARTIAL=", StringComparison.OrdinalIgnoreCase)) {
          var partial = segments[i].Substring(9);

          if (partial.EndsWith("/", StringComparison.Ordinal))
            partial = partial.Substring(0, partial.Length - 1);

          try {
            var splitted = partial.Split('.');

            if (splitted.Length == 1)
              return new ImapPartialRange(long.Parse(splitted[0]));
            else if (splitted.Length == 2)
              return new ImapPartialRange(long.Parse(splitted[0]), long.Parse(splitted[1]));
            else
              throw new UriFormatException("malformed partial range");

          }
          catch (FormatException) {
            throw new UriFormatException("malformed partial range");
          }
          catch (ArgumentOutOfRangeException) {
            throw new UriFormatException("malformed partial range");
          }
        }
      }

      return null;
    }

    /*
     *    iurlauth        = iurlauth-rump iua-verifier
     * 
     *    iua-verifier    = ":" uauth-mechanism ":" enc-urlauth
     * 
     *    iurlauth-rump   = [expire] ";URLAUTH=" access
     * 
     *    access          = ("submit+" enc-user) / ("user+" enc-user) /
     *                        "authuser" / "anonymous"
     * 
     *    expire          = ";EXPIRE=" date-time
     *                          ; date-time is defined in [DATETIME]
     * 
     *    uauth-mechanism = "INTERNAL" / 1*(ALPHA / DIGIT / "-" / ".")
     *                         ; Case-insensitive.
     *                         ; New mechanisms MUST be registered with IANA.
     */

    /*
     * TODO: URLAUTH
     */

    /*
     *    iabsolute-path  = "/" [ icommand ]
     *                ; icommand, if present, MUST NOT start with '/'.
     *                ;
     *                ; Corresponds to 'path-absolute [ "?" query ]'
     *                ; in [URI-GEN]
     * 
     *    icommand         = imessagelist /
     *                       imessagepart [iurlauth]
     * 
     *    imailbox-ref     = enc-mailbox [uidvalidity]
     * 
     *    imessagelist     = imailbox-ref [ "?" enc-search ]
     *                   ; "enc-search" is [URI-GEN] "query".
     * 
     *    imessagepart     = imailbox-ref iuid [isection] [ipartial]
     */

    /// <summary>returns URI form</summary>
    public static ImapUriForm GetUriForm(Uri uri)
    {
      CheckUriScheme(uri);

      var path = uri.AbsolutePath;
      var isPathEmpty = (path == "/");
      var mailboxSpecified = !isPathEmpty && !path.StartsWith("/;", StringComparison.Ordinal);

      if (!string.IsNullOrEmpty(uri.Query)) {
        if (mailboxSpecified)
          return ImapUriForm.SearchMessages;
        else
          return ImapUriForm.Unknown;
      }
      else if (0 <= path.IndexOf("/;UID=", StringComparison.OrdinalIgnoreCase)) {
        if (mailboxSpecified)
          return ImapUriForm.FetchMessage;
        else
          return ImapUriForm.Unknown;
      }
      else {
        if (mailboxSpecified)
          return ImapUriForm.ListMessages;
        else if (isPathEmpty)
          return ImapUriForm.Server;
        else
          return ImapUriForm.Unknown;
      }
    }

    /*
    get
      {
        if (Uid != 0L)
          return ImapUriForm.FetchMessage;
        else if (!string.IsNullOrEmpty(Query))
          return ImapUriForm.SearchMessages;
        else if (!string.IsNullOrEmpty(Mailbox))
          return ImapUriForm.ListMessages;
        else if (!string.IsNullOrEmpty(Host))
          return ImapUriForm.Server;
        else
          return ImapUriForm.Relative;
      }
    }
    */

    private static void CheckUriScheme(Uri uri)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      if (ImapUri.IsImap(uri))
        return;
      else
        throw new ArgumentException("uri is not IMAP URL", "uri");
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

    public ImapStyleUriParser()
      : base(parserOptions)
    {
    }

    protected override void InitializeAndValidate (Uri uri, out UriFormatException parsingError)
    {
      // TODO: check UIDVALIDITY, UID, SECTION, PARTIAL, etc.
      base.InitializeAndValidate(uri, out parsingError);
    }
  }
}
