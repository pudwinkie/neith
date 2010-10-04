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

namespace Smdn.Security.Authentication.Sasl.Client {
  /*
   * RFC 4505 - Anonymous Simple Authentication and Security Layer (SASL) Mechanism
   * http://tools.ietf.org/html/rfc4505
   */
  [SaslMechanism(SaslMechanisms.Anonymous, true)]
  public class AnonymousMechanism : SaslClientMechanism {
    public override bool ClientFirst {
      get { return true; }
    }

    protected override SaslExchangeStatus Exchange(ByteString serverChallenge, out ByteString clientResponse)
    {
      // 2. The Anonymous Mechanism
      //    The mechanism consists of a single message from the client to the
      //    server.  The client may include in this message trace information in
      //    the form of a string of [UTF-8]-encoded [Unicode] characters prepared
      //    in accordance with [StringPrep] and the "trace" stringprep profile
      //    defined in Section 3 of this document.  The trace information, which
      //    has no semantical value, should take one of two forms: an Internet
      //    email address, or an opaque string that does not contain the '@'
      //    (U+0040) character and that can be interpreted by the system
      //    administrator of the client's domain.  For privacy reasons, an
      //    Internet email address or other information identifying the user
      //    should only be used with permission from the user.
      if (Credential == null)
        throw new SaslException("Credential property must be set");

      clientResponse = null;

      if (string.IsNullOrEmpty(Credential.UserName))
        return SaslExchangeStatus.Failed;

      // XXX
      clientResponse = new ByteString(Encoding.UTF8.GetBytes(Credential.UserName));

      return SaslExchangeStatus.Succeeded;
    }
  }
}
