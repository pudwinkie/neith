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

namespace Smdn.Security.Authentication.Sasl.Client {
  /*
   * RFC 4616 - The PLAIN Simple Authentication and Security Layer (SASL) Mechanism
   * http://tools.ietf.org/html/rfc4616
   */
  [SaslMechanism(SaslMechanisms.Plain, true)]
  public class PlainMechanism : SaslClientMechanism {
    public override bool ClientFirst {
      get { return true; }
    }

    protected override SaslExchangeStatus Exchange(ByteString serverChallenge, out ByteString clientResponse)
    {
      if (Credential == null)
        throw new SaslException("Credential property must be set");

      clientResponse = null;

      if (string.IsNullOrEmpty(Credential.UserName) || string.IsNullOrEmpty(Credential.Password))
        return SaslExchangeStatus.Failed;

      var responseBuilder = new ByteStringBuilder(0x300); // 255(authcid) + 1(NUL) + 255(authzid) + 1(NUL) + 255(passwd)

      responseBuilder.Append(Encoding.UTF8.GetBytes(Credential.Domain ?? string.Empty)); // authcid
      responseBuilder.Append(Smdn.Formats.Octets.NUL);
      responseBuilder.Append(Encoding.UTF8.GetBytes(Credential.UserName)); // authzid
      responseBuilder.Append(Smdn.Formats.Octets.NUL);
      responseBuilder.Append(Encoding.UTF8.GetBytes(Credential.Password)); // passwd

      clientResponse = responseBuilder.ToByteString(false);

      return SaslExchangeStatus.Succeeded;
    }
  }
}
