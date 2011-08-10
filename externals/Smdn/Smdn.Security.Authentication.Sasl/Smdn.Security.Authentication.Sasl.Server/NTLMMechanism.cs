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

#if false

using System;

namespace Smdn.Security.Authentication.Sasl.Server {
  /*
   * [MS-NLMP]: NT LAN Manager (NTLM) Authentication Protocol Specification
   * http://msdn.microsoft.com/en-us/library/cc236621(PROT.13).aspx
   * 
   * Microsoft Open Specification Support Team Blog : NTLM Overview
   * http://blogs.msdn.com/openspecification/archive/2009/05/01/ntlm-overview.aspx
   * 
   * The NTLM Authentication Protocol and Security Support Provider
   * http://davenport.sourceforge.net/ntlm.html
   */
  [SaslMechanismName("NTLM")]
  public class NTLMMechanism : SaslServerMechanism {
    public NTLMMechanism()
    {
      impl = NTLMMechanismImpl.Create();
    }

    public override void Initialize()
    {
      if (impl != null)
        impl.Dispose();

      impl = NTLMMechanismImpl.Create();

      step = 0;

      base.Initialize();
    }

    protected override SaslExchangeStatus Exchange(ByteString clientResponse, out ByteString serverChallenge)
    {
      /*
      if (Credential == null)
        throw new SaslException("Credential property must be set");
      */

      switch (step) {
        case 0: { // receive NTLM negotiate message (Type 1) and send NTLM challenge message (Type 2)
          byte[] response;

          //impl.Credential = this.Credential;
          impl.Initialize();
          impl.Negotiate(clientResponse.ByteArray, out response);

          step++;

          serverChallenge = new ByteString(response);

          return SaslExchangeStatus.Continuing;
        }

        case 1: { // receive NTLM authenticate message (Type 3)
          byte[] response;

          impl.Authenticate(clientResponse.ByteArray, out response);

          serverChallenge = new ByteString(response);

          step++;

          return SaslExchangeStatus.Succeeded;
        }

        default:
          serverChallenge = null;
          return SaslExchangeStatus.Failed; // unexpected server challenge
      }
    }

    private int step;
    private NTLMMechanismImpl impl;
  }
}

#endif
