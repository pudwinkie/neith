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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Smdn.Security.Authentication.Sasl.Client {
  /*
   * RFC 2831 - Using Digest Authentication as a SASL Mechanism
   * http://tools.ietf.org/html/rfc2831
   */
  [SaslMechanism(SaslMechanisms.DigestMD5, false)]
  public class DigestMD5Mechanism : SaslClientMechanism {
    public byte[] Cnonce {
      get; set;
    }

    public DigestMD5Mechanism()
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      Cnonce = null;

      if (md5 == null)
        md5 = MD5.Create();
      else
        md5.Initialize();

      step = 0;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (md5 != null) {
          md5.Clear();
          md5 = null;
        }
      }

      base.Dispose(disposing);
    }

    protected override SaslExchangeStatus Exchange(ByteString serverChallenge, out ByteString clientResponse)
    {
      if (md5 == null)
        throw new ObjectDisposedException(GetType().FullName);
      if (Credential == null)
        throw new SaslException("Credential property must be set");
      if (string.IsNullOrEmpty(ServiceName))
        throw new SaslException("ServiceName property must be set");

      var pairs = ParseChallenge(serverChallenge);

      if (step == 3) {
        /* 
         * 2.1.3 Step Three
         */
        if (pairs.ContainsKey("rspauth")) {
          clientResponse = ByteString.CreateEmpty();
          return SaslExchangeStatus.Succeeded;
        }
        else {
          clientResponse = null;
          return SaslExchangeStatus.Failed;
        }
      }

      /*
       * 2.1.1 Step One
       */
      step = 1;
      clientResponse = null;

      if (string.IsNullOrEmpty(Credential.UserName) || string.IsNullOrEmpty(Credential.Password))
        return SaslExchangeStatus.Failed;

      //    algorithm
      //       This directive is required for backwards compatibility with HTTP
      //       Digest., which supports other algorithms. . This directive is
      //       required and MUST appear exactly once; if not present, or if
      //       multiple instances are present, the client should abort the
      //       authentication exchange.
      //
      //         algorithm         = "algorithm" "=" "md5-sess"
      if (!(pairs.ContainsKey("algorithm") && pairs["algorithm"].Equals("md5-sess")))
        return SaslExchangeStatus.Failed; // "algorithm" is missing or unsupported algorithm

      //    charset
      //       This directive, if present, specifies that the server supports
      //       UTF-8 encoding for the username and password. If not present, the
      //       username and password must be encoded in ISO 8859-1 (of which
      //       US-ASCII is a subset). The directive is needed for backwards
      //       compatibility with HTTP Digest, which only supports ISO 8859-1.
      //       This directive may appear at most once; if multiple instances are
      //       present, the client should abort the authentication exchange.
      //
      //         charset           = "charset" "=" "utf-8"
      var charset = (pairs.ContainsKey("charset") && pairs["charset"].Equals("utf-8"))
        ? Encoding.UTF8
        : Encoding.GetEncoding("iso-8859-1");

      ByteString realm, nonce, qop;

      if (!pairs.TryGetValue("realm", out realm))
        return SaslExchangeStatus.Failed;
      if (!pairs.TryGetValue("nonce", out nonce))
        return SaslExchangeStatus.Failed;
      if (!pairs.TryGetValue("qop", out qop))
        return SaslExchangeStatus.Failed;

      qop = new ByteString("auth"); // ignore qop-options

      /*
       * 2.1.2 Step Two
       */
      step = 2;

      //    cnonce
      //       It is RECOMMENDED that it contain at least 64 bits of entropy.
      var cnonce = new ByteString(Cnonce ?? Nonce.Generate(64, true));

      //    nonce-count
      //       The nc-value is the hexadecimal count of the number of requests
      //       (including the current request) that the client has sent with the
      //       nonce value in this request.  For example, in the first request
      //       sent in response to a given nonce value, the client sends
      //       "nc=00000001"
      var nc = new ByteString((1).ToString("X8"));

      //    digest-uri
      //       Indicates the principal name of the service with which the client
      //       wishes to connect, formed from the serv-type, host, and serv-name.
      //       For example, the FTP service on "ftp.example.com" would have a
      //       "digest-uri" value of "ftp/ftp.example.com"; the SMTP server from
      //       the example above would have a "digest-uri" value of
      //       "smtp/mail3.example.com/example.com".
      var digestUri = new ByteString(string.Format("{0}/{1}", ServiceName, Credential.Domain ?? string.Empty));

      //       response-value  =
      //          HEX( KD ( HEX(H(A1)),
      //                  { nonce-value, ":" nc-value, ":",
      //                    cnonce-value, ":", qop-value, ":", HEX(H(A2)) }))
      var responseValue = HexKD(md5,
                                HexHA1(md5, charset, Credential.UserName, realm, Credential.Password, nonce, cnonce, null),
                                nonce,
                                nc,
                                cnonce,
                                qop,
                                HexHA2(md5, qop, digestUri));

      var responseBuilder = new ByteStringBuilder(0x200);

      if (charset == Encoding.UTF8)
        responseBuilder.Append("charset=utf-8,");

      responseBuilder.Append("username=\"");
      responseBuilder.Append(Credential.UserName);
      responseBuilder.Append("\",realm=\"");
      responseBuilder.Append(realm);
      responseBuilder.Append("\",nonce=\"");
      responseBuilder.Append(nonce);
      responseBuilder.Append("\",nc=");
      responseBuilder.Append(nc);
      responseBuilder.Append(",cnonce=\"");
      responseBuilder.Append(cnonce);
      responseBuilder.Append("\",digest-uri=\"");
      responseBuilder.Append(digestUri);
      responseBuilder.Append("\",response=");
      responseBuilder.Append(responseValue);
      responseBuilder.Append(",qop=");
      responseBuilder.Append(qop);

      step = 3;

      clientResponse = responseBuilder.ToByteString();

      return SaslExchangeStatus.Continuing;
    }

    private static ByteString HexHA1(MD5 h, Encoding charset, string username, ByteString realm, string password, ByteString nonce, ByteString cnonce, ByteString authzid)
    {
      //    If authzid is specified, then A1 is
      // 
      //       A1 = { H( { username-value, ":", realm-value, ":", passwd } ),
      //            ":", nonce-value, ":", cnonce-value, ":", authzid-value }
      // 
      //    If authzid is not specified, then A1 is
      // 
      //       A1 = { H( { username-value, ":", realm-value, ":", passwd } ),
      //            ":", nonce-value, ":", cnonce-value }
      var a1 = new ByteStringBuilder(0x100);

      a1.Append(h.ComputeHash(ArrayExtensions.Concat(charset.GetBytes(username),
                                                     new byte[] {0x3a},
                                                     realm.ByteArray,
                                                     new byte[] {0x3a},
                                                     charset.GetBytes(password))));
      a1.Append(0x3a);
      a1.Append(nonce);
      a1.Append(0x3a);
      a1.Append(cnonce);

      if (authzid != null) {
        a1.Append(0x3a);
        a1.Append(authzid);
      }

      return HexH(h, a1.ToByteString());
    }

    private static ByteString HexHA2(MD5 h, ByteString qop, ByteString digestUri)
    {
      //    If the "qop" directive's value is "auth", then A2 is:
      // 
      //       A2       = { "AUTHENTICATE:", digest-uri-value }
      // 
      //    If the "qop" value is "auth-int" or "auth-conf" then A2 is:
      // 
      //       A2       = { "AUTHENTICATE:", digest-uri-value,
      //                ":00000000000000000000000000000000" }
      var a2 = new ByteStringBuilder(0x80);

      a2.Append("AUTHENTICATE:");
      a2.Append(digestUri);

      if (qop.EqualsIgnoreCase("auth-int") || qop.EqualsIgnoreCase("auth-conf"))
        a2.Append(":00000000000000000000000000000000");

      return HexH(h, a2.ToByteString());
    }

    private static ByteString HexKD(MD5 h, ByteString hexHA1, ByteString nonce, ByteString nc, ByteString cnonce, ByteString qop, ByteString hexHA2)
    {
      //       response-value  =
      //          HEX( KD ( HEX(H(A1)),
      //                  { nonce-value, ":" nc-value, ":",
      //                    cnonce-value, ":", qop-value, ":", HEX(H(A2)) }))

      //    Let KD(k, s) be H({k, ":", s}), i.e., the 16 octet hash of the string
      //    k, a colon and the string s.
      var tmp = new ByteStringBuilder(0x100);

      tmp.Append(hexHA1);
      tmp.Append(0x3a);
      tmp.Append(nonce);
      tmp.Append(0x3a);
      tmp.Append(nc);
      tmp.Append(0x3a);
      tmp.Append(cnonce);
      tmp.Append(0x3a);
      tmp.Append(qop);
      tmp.Append(0x3a);
      tmp.Append(hexHA2);

      return HexH(h, tmp.ToByteString());
    }

    private static ByteString HexH(MD5 h, ByteString s)
    {
      //    Let H(s) be the 16 octet MD5 hash [RFC 1321] of the octet string s.

      //    Let HEX(n) be the representation of the 16 octet MD5 hash n as a
      //    string of 32 hex digits (with alphabetic characters always in lower
      //    case, since MD5 is case sensitive).
      return new ByteString(Smdn.Formats.Hexadecimals.ToLowerByteArray(h.ComputeHash(s.ByteArray)));
    }

    private static Dictionary<string, ByteString> ParseChallenge(ByteString str)
    {
      // quoted-string  = ( <"> qdstr-val <"> )
      // qdstr-val      = *( qdtext | quoted-pair )
      // qdtext         = <any TEXT except <">>
      // quoted-pair    = "\" CHAR
      var splitAt = new List<int>();
      var quoted = false;

      for (var i = 0; i < str.Length; i++) {
        if (str[i] == '\\') // escape
          i++;
        else if (str[i] == '\"')
          quoted = !quoted;
        else if (!quoted && str[i] == ',')
          splitAt.Add(i);
      }

      splitAt.Add(str.Length); // end of line

      var pairs = new ByteString[splitAt.Count];

      for (var i = 0; i < pairs.Length; i++) {
        if (i == 0)
          pairs[i] = str.Substring(0, splitAt[i]).Trim();
        else
          pairs[i] = str.Substring(splitAt[i - 1] + 1, splitAt[i] - splitAt[i - 1] - 1).Trim();
      }

      var ret = new Dictionary<string, ByteString>(StringComparer.Ordinal);

      foreach (var p in Array.ConvertAll(pairs, delegate(ByteString pair) {
        var delim = pair.IndexOf('=');
        var dequote = (pair[delim + 1] == '\"') && pair.EndsWith("\"");

        if (dequote)
          return new KeyValuePair<string, ByteString>(pair.Substring(0, delim).ToString(),
                                                      pair.Substring(delim + 2, pair.Length - (delim + 3)));
        else
          return new KeyValuePair<string, ByteString>(pair.Substring(0, delim).ToString(),
                                                      pair.Substring(delim + 1));
      })) {
        // TODO: quoted-pair
        ret.Add(p.Key, p.Value);
      }

      return ret;
    }

    private int step;
    private MD5 md5 = null;
  }
}