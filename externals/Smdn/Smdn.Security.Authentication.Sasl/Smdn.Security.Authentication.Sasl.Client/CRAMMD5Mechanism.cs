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
using System.Security.Cryptography;
using System.Text;

namespace Smdn.Security.Authentication.Sasl.Client {
  /*
   * RFC 2195 IMAP/POP AUTHorize Extension for Simple Challenge/Response
   * http://tools.ietf.org/html/rfc2195
   */
  [SaslMechanism(SaslMechanisms.CRAMMD5, false)]
  public class CRAMMD5Mechanism : SaslClientMechanism {
    public override void Initialize()
    {
      base.Initialize();

      if (md5 == null)
        md5 = new HMACMD5();
      else
        md5.Initialize();
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

      // 2. Challenge-Response Authentication Mechanism (CRAM)

      // The data encoded in the first ready response contains an
      // presumptively arbitrary string of random digits, a timestamp, and the
      // fully-qualified primary host name of the server.  The syntax of the
      // unencoded form must correspond to that of an RFC 822 'msg-id'
      // [RFC822] as described in [POP3].

      // The client makes note of the data and then responds with a string
      // consisting of the user name, a space, and a 'digest'.  The latter is
      // computed by applying the keyed MD5 algorithm from [KEYED-MD5] where
      // the key is a shared secret and the digested text is the timestamp
      // (including angle-brackets).
      //
      // This shared secret is a string known only to the client and server.
      if (Credential == null)
        throw new SaslException("Credential property must be set");

      clientResponse = null;

      if (string.IsNullOrEmpty(Credential.UserName) || string.IsNullOrEmpty(Credential.Password))
        return SaslExchangeStatus.Failed;

      md5.Key = Encoding.ASCII.GetBytes(Credential.Password ?? string.Empty);

      var keyed = md5.ComputeHash(serverChallenge.Segment.Array,
                                  serverChallenge.Segment.Offset,
                                  serverChallenge.Segment.Count);

      // The `digest' parameter itself is a 16-octet value which is sent in
      // hexadecimal format, using lower-case ASCII characters.
      var digest = Smdn.Formats.Hexadecimals.ToLowerString(keyed);

      clientResponse = ByteString.CreateMutable(string.Concat(Credential.UserName, " ", digest));

      return SaslExchangeStatus.Succeeded;
    }

    private HMACMD5 md5 = null;
  }
}
