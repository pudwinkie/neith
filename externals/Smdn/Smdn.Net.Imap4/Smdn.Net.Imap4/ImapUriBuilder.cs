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

using Smdn;
using Smdn.Formats;

namespace Smdn.Net.Imap4 {
  public class ImapUriBuilder :
    ICloneable,
    IEquatable<ImapUriBuilder>,
    IEquatable<Uri>
  {
    /// <value>The scheme of the IMAP URL.</value>
    public string Scheme {
      get { return scheme; }
      set
      {
        if (value == null)
          scheme = ImapUri.UriSchemeImap;

        if (!string.Equals(value, ImapUri.UriSchemeImap, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(value, ImapUri.UriSchemeImaps, StringComparison.OrdinalIgnoreCase))
          throw new ArgumentException(string.Format("scheme must be '{0}' or '{1}'", ImapUri.UriSchemeImap, ImapUri.UriSchemeImaps), "Scheme");

        scheme = value;
        uri = null;
      }
    }

    /// <value>The host name of the IMAP URL.</value>
    public string Host {
      get { return host; }
      set { host = value ?? string.Empty; uri = null; }
    }

    /// <value>The port number of the IMAP URL.</value>
    /// <remarks>If a port is not specified as part of the IMAP URL, the <see cref="Port"/> property returns value -1.</remarks>
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

    /// <value>The user name of the user that accesses the IMAP URL. </value>
    public string UserName {
      get { return userName; }
      set { userName = value ?? string.Empty; uri = null; }
    }

    /// <value>The authentication mechanism that is used to access the IMAP URL.</value>
    public ImapAuthenticationMechanism AuthType {
      get { return authType; }
      set { authType = value; uri = null; }
    }

    /// <value>The decoded mailbox name of the IMAP URL.</value>
    public string Mailbox {
      get { return mailbox; }
      set { mailbox = value ?? string.Empty; uri = null; }
    }

    /// <value>The search criteria included in the IMAP URL.</value>
    public IImapUrlSearchQuery SearchCriteria {
      get { return searchCriteria; }
      set { searchCriteria = value; uri = null; }
    }

    public Encoding Charset {
      get { return charset; }
      set { charset = value; uri = null; }
    }

    /// <value>The value of ';UIDVALIDITY=' modifier included in the IMAP URL.</value>
    /// <remarks>If a UIDVALIDITY is not specified as part of the IMAP URL, the <see cref="UidValidity"/> property returns value 0.</remarks>
    public long UidValidity {
      get { return uidValidity; }
      set
      {
        if (value < 0L)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("UidValidity", value);
        uidValidity = value;
        uri = null;
      }
    }

    /// <value>The value of ';UID=' parameter included in the IMAP URL.</value>
    /// <remarks>If a UID is not specified as part of the IMAP URL, the <see cref="Uid"/> property returns value 0.</remarks>
    public long Uid {
      get { return uid; }
      set
      {
        if (value < 0L)
          throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("Uid", value);
        uid = value;
        uri = null;
      }
    }

    /// <value>The value of ';SECTION=' parameter included in the IMAP URL.</value>
    public string Section {
      get { return section; }
      set { section = value ?? string.Empty; uri = null; }
    }

    /// <value>The value of ';PARTIAL=' parameter included in the IMAP URL.</value>
    public ImapPartialRange? Partial {
      get { return @partial; }
      set { @partial = value; uri = null; }
    }

    /// <value>A <see cref="System.Uri"/> that contains the URI constructed by the <see cref="ImapUriBuilder"/>.</value>
    public Uri Uri {
      get
      {
        if (uri != null)
          return uri;

        if ((searchCriteria != null || uidValidity != 0L || uid != 0L) && mailbox.Length == 0)
          throw new UriFormatException("Mailbox must be specified");
        if ((section.Length != 0 || @partial != null) && uid == 0L)
          throw new UriFormatException("Uid must be specified");

        uri = new Uri(ToString());

        return uri;
      }
    }

    public ImapUriBuilder(string uri)
      : this(new Uri(uri), (Converter<Uri, IImapUrlSearchQuery>)null)
    {
    }

    public ImapUriBuilder(string uri, Converter<Uri, IImapUrlSearchQuery> parseQuery)
      : this(new Uri(uri), parseQuery)
    {
    }

    public ImapUriBuilder(Uri uri)
      : this(uri, (Converter<Uri, IImapUrlSearchQuery>)null)
    {
    }

    public ImapUriBuilder(Uri uri, Converter<Uri, IImapUrlSearchQuery> parseQuery)
    {
      if (uri == null)
        throw new ArgumentNullException("uri");

      Scheme = uri.Scheme;
      Host = uri.Host;
      Port = uri.Port;
      UserName        = ImapStyleUriParser.GetUser(uri);
      AuthType        = ImapStyleUriParser.GetAuthType(uri);
      Mailbox         = ImapStyleUriParser.GetMailbox(uri);
      SearchCriteria  = (parseQuery == null) ? null : parseQuery(uri);
      UidValidity     = ImapStyleUriParser.GetUidValidity(uri);
      Uid             = ImapStyleUriParser.GetUid(uri);
      Section         = ImapStyleUriParser.GetSection(uri);
      Partial         = ImapStyleUriParser.GetPartial(uri);
    }

#region "constructors for 'imap://<iserver>[/]'"
    public ImapUriBuilder()
      : this(false, "localhost", -1, null, null)
    {
    }

    public ImapUriBuilder(bool imapsScheme, string host)
      : this(imapsScheme, host, -1, null, null)
    {
    }

    public ImapUriBuilder(string host, int port)
      : this(false, host, port, null, null)
    {
    }

    public ImapUriBuilder(bool imapsScheme, string host, int port)
      : this(imapsScheme, host, port, null, null)
    {
    }

    public ImapUriBuilder(string host, string userName, ImapAuthenticationMechanism authType)
      : this(false, host, -1, userName, authType)
    {
    }

    public ImapUriBuilder(bool imapsScheme, string host, string userName, ImapAuthenticationMechanism authType)
      : this(imapsScheme, host, -1, userName, authType)
    {
    }

    public ImapUriBuilder(string host, int port, string userName, ImapAuthenticationMechanism authType)
      : this(false, host, port, userName, authType)
    {
    }

    public ImapUriBuilder(bool imapsScheme, string host, int port, string userName, ImapAuthenticationMechanism authType)
    {
      Scheme = imapsScheme ? ImapUri.UriSchemeImaps : ImapUri.UriSchemeImap;
      Host = host;
      Port = port;
      UserName = userName;
      AuthType = authType;
    }
#endregion

#region "constructors for 'imap://<iserver>/<enc-mailbox>[<uidvalidity>]'"
    public ImapUriBuilder(Uri baseUri, string mailbox)
      : this(baseUri, mailbox, 0L)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox)
      : this(baseUri, mailbox, 0L)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uidValidity)
      : this(baseUri)
    {
      Mailbox = mailbox;
      UidValidity = uidValidity;
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uidValidity)
      : this(baseUri)
    {
      Mailbox = mailbox;
      UidValidity = uidValidity;
    }
#endregion

#region "constructors for 'imap://<iserver>/<enc-mailbox>[<uidvalidity>][?<enc-search>]'"
    public ImapUriBuilder(Uri baseUri, string mailbox, IImapUrlSearchQuery searchCriteria)
      : this(baseUri, mailbox, 0L, searchCriteria, null)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, IImapUrlSearchQuery searchCriteria)
      : this(baseUri, mailbox, 0L, searchCriteria, null)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uidValidity, IImapUrlSearchQuery searchCriteria)
      : this(baseUri, mailbox, uidValidity, searchCriteria, null)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uidValidity, IImapUrlSearchQuery searchCriteria)
      : this(baseUri, mailbox, uidValidity, searchCriteria, null)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, IImapUrlSearchQuery searchCriteria, Encoding charset)
      : this(baseUri, mailbox, 0L, searchCriteria, charset)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, IImapUrlSearchQuery searchCriteria, Encoding charset)
      : this(baseUri, mailbox, 0L, searchCriteria, charset)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uidValidity, IImapUrlSearchQuery searchCriteria, Encoding charset)
      : this(baseUri)
    {
      Mailbox = mailbox;
      UidValidity = uidValidity;
      SearchCriteria = searchCriteria;
      Charset = charset;
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uidValidity, IImapUrlSearchQuery searchCriteria, Encoding charset)
      : this(baseUri)
    {
      Mailbox = mailbox;
      UidValidity = uidValidity;
      SearchCriteria = searchCriteria;
      Charset = charset;
    }
#endregion

#region "constructors for 'imap://<iserver>/<enc-mailbox>[<uidvalidity>]<iuid>[<isection>][<ipartial>][<iurlauth>]'"
    public ImapUriBuilder(Uri baseUri, string mailbox, long uidValidity, long uid)
      : this(baseUri, mailbox, uidValidity, uid, null, null)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uidValidity, long uid)
      : this(baseUri, mailbox, uidValidity, uid, null, null)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uid, string section)
      : this(baseUri, mailbox, 0L, uid, section, null)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uid, string section)
      : this(baseUri, mailbox, 0L, uid, section, null)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uidValidity, long uid, string section)
      : this(baseUri, mailbox, uidValidity, uid, section, null)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uidValidity, long uid, string section)
      : this(baseUri, mailbox, uidValidity, uid, section, null)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uid, string section, ImapPartialRange? @partial)
      : this(baseUri, mailbox, 0L, uid, section, @partial)
    {
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uid, string section, ImapPartialRange? @partial)
      : this(baseUri, mailbox, 0L, uid, section, @partial)
    {
    }

    public ImapUriBuilder(Uri baseUri, string mailbox, long uidValidity, long uid, string section, ImapPartialRange? @partial)
      : this(baseUri)
    {
      Mailbox = mailbox;
      UidValidity = uidValidity;
      Uid = uid;
      Section = section;
      Partial = @partial;
    }

    public ImapUriBuilder(string baseUri, string mailbox, long uidValidity, long uid, string section, ImapPartialRange? @partial)
      : this(baseUri)
    {
      Mailbox = mailbox;
      UidValidity = uidValidity;
      Uid = uid;
      Section = section;
      Partial = @partial;
    }
#endregion

#region "equality"
    public override bool Equals(object obj)
    {
      var builder = obj as ImapUriBuilder;

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

    public bool Equals(ImapUriBuilder other)
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

    public ImapUriBuilder Clone()
    {
      var cloned = MemberwiseClone() as ImapUriBuilder;

      if (this.authType != null)
        cloned.authType = ImapAuthenticationMechanism.GetKnownOrCreate(this.authType.Value);

      return cloned;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    public override string ToString()
    {
      /*
       * http://tools.ietf.org/html/rfc5092
       * RFC 5092 - IMAP URL Scheme
       * 
       *    An absolute IMAP URL takes one of the following forms:
       * 
       *       imap://<iserver>[/]
       * 
       *       imap://<iserver>/<enc-mailbox>[<uidvalidity>][?<enc-search>]
       * 
       *       imap://<iserver>/<enc-mailbox>[<uidvalidity>]<iuid>
       *        [<isection>][<ipartial>][<iurlauth>]
       * 
       * http://tools.ietf.org/html/rfc3986
       * RFC 3986 - Uniform Resource Identifier (URI): Generic Syntax
       */
      var sb = new StringBuilder(1024);

      sb.Append(scheme);
      sb.Append(Uri.SchemeDelimiter);

      /*
       *    iserver          = [iuserinfo "@"] host [ ":" port ]
       *                            ; This is the same as "authority" defined
       *                            ; in [URI-GEN].  See [URI-GEN] for "host"
       *                            ; and "port" definitions.
       *    iuserinfo        = enc-user [iauth] / [enc-user] iauth
       *                                 ; conforms to the generic syntax of
       *                                 ; "userinfo" as defined in [URI-GEN].
       *    iauth            = ";AUTH=" ( "*" / enc-auth-type )
       *    enc-user         = 1*achar
       *                   ; %-encoded version of [IMAP4] authorization
       *                   ; identity or "userid".
       *    enc-auth-type    = 1*achar
       *                    ; %-encoded version of [IMAP4] "auth-type"
       */
      sb.Append(PercentEncoding.GetEncodedString(userName,
                                                 ToPercentEncodedTransformMode.Rfc5092Uri));

      if (authType != null) {
        sb.Append(";AUTH=");

        if (authType == ImapAuthenticationMechanism.SelectAppropriate)
          sb.Append(authType);
        else
          sb.Append(PercentEncoding.GetEncodedString(authType.Value,
                                                     ToPercentEncodedTransformMode.Rfc5092Uri));
      }

      if (0 < userName.Length || authType != null)
        sb.Append('@');

      sb.Append(host);

      if (0 <= port) {
        sb.Append(':');
        sb.Append(port);
      }

      sb.Append('/');

      if (0 < mailbox.Length) {
        /*
         *    enc-mailbox      = 1*bchar
         *                   ; %-encoded version of [IMAP4] "mailbox"
         */
        var encMailbox = PercentEncoding.GetEncodedString(mailbox,
                                                          ToPercentEncodedTransformMode.Rfc5092Path,
                                                          Encoding.UTF8);

        if (encMailbox.StartsWith("/", StringComparison.Ordinal)) {
          sb.Append("%2f");
          sb.Append(encMailbox.Substring(1));
        }
        else {
          sb.Append(encMailbox);
        }

        if (uidValidity != 0L) {
          sb.Append(";UIDVALIDITY=");
          sb.Append(uidValidity);
        }
      }

      if (uid != 0L) {
        if (0 < mailbox.Length)
          sb.Append('/');

        sb.Append(";UID=");
        sb.Append(uid);

        /*
         *    enc-section      = 1*bchar
         *                   ; %-encoded version of [IMAP4] "section-spec"
         */
        if (0 < section.Length) {
          sb.Append("/;SECTION=");
          sb.Append(PercentEncoding.GetEncodedString(section,
                                                     ToPercentEncodedTransformMode.Rfc5092Path));
        }

        if (@partial != null)
          sb.Append(@partial.Value.ToString("u", null));
      }
      else if (searchCriteria != null) {
        sb.Append('?');

        /*
         *    enc-search       = 1*bchar
         *                            ; %-encoded version of [IMAPABNF]
         *                            ; "search-program".  Note that IMAP4
         *                            ; literals may not be used in
         *                            ; a "search-program", i.e., only
         *                            ; quoted or non-synchronizing
         *                            ; literals (if the server supports
         *                            ; LITERAL+ [LITERAL+]) are allowed.
         */
        byte[] searchQuery;
        bool charsetSpecified;

        try {
          searchQuery = searchCriteria.GetEncodedQuery(charset, out charsetSpecified);
        }
        catch (EncoderFallbackException) {
          throw new InvalidOperationException("Charset must be specified");
        }

        if (charset != null && charsetSpecified) {
          sb.Append("CHARSET%20");
          sb.Append(charset.WebName);
          sb.Append("%20");
        }

        sb.Append(PercentEncoding.GetEncodedString(searchQuery,
                                                   ToPercentEncodedTransformMode.Rfc5092Path));
      }

      return sb.ToString();
    }

    private Uri uri;

    private string scheme = ImapUri.UriSchemeImap;
    private string host = string.Empty;
    private int port = -1;
    private string userName = string.Empty;
    private ImapAuthenticationMechanism authType = null;
    private string mailbox = string.Empty;
    private IImapUrlSearchQuery searchCriteria = null;
    private Encoding charset = null;
    private long uidValidity = 0L;
    private long uid = 0L;
    private string section = string.Empty;
    private ImapPartialRange? @partial = null;
  }
}
