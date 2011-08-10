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

using Mono.Security.Protocol.Ntlm;

namespace Smdn.Security.Authentication.Sasl.Client {
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
  [SaslMechanism(SaslMechanisms.NTLM, false)]
  public class NTLMMechanism : SaslClientMechanism {
    public override bool ClientFirst {
      get { return true; }
    }

    public string TargetHost {
      get; set;
    }

    public NTLMMechanism()
    {
    }

    public override void Initialize()
    {
      base.Initialize();

      step = 0;
    }

    protected override SaslExchangeStatus Exchange(ByteString serverChallenge, out ByteString clientResponse)
    {
      if (Credential == null)
        throw new SaslException("Credential property must be set");

      clientResponse = null;

      switch (step) {
        case 0: { // send NTLM negotiate message (Type 1)
          const NtlmFlags type1Flags =
            NtlmFlags.RequestTarget |
            NtlmFlags.NegotiateNtlm |
            NtlmFlags.NegotiateUnicode |
            NtlmFlags.NegotiateOem |
            NtlmFlags.NegotiateDomainSupplied |
            NtlmFlags.NegotiateWorkstationSupplied;

          var type1 = new Type1Message();

          type1.Flags = type1Flags;
          type1.Host = TargetHost ?? string.Empty; // ?
          type1.Domain = Credential.Domain ?? string.Empty;

          clientResponse = ByteString.CreateImmutable(type1.GetBytes());

          step++;

          return SaslExchangeStatus.Continuing;
        }

        case 1: { // receive NTLM challenge message (Type 2) and send NTLM authenticate message (Type 3)
          if (string.IsNullOrEmpty(Credential.UserName) || string.IsNullOrEmpty(Credential.Password))
            return SaslExchangeStatus.Failed;

          var type2 = new Type2Message(serverChallenge.ToArray());
          var type3 = new Type3Message();

          type3.Flags = NtlmFlags.NegotiateNtlm | NtlmFlags.NegotiateUnicode; // XXX
          type3.Host = TargetHost ?? string.Empty; // ?
          type3.Domain = Credential.Domain ?? string.Empty;

          type3.Challenge = type2.Nonce;
          type3.Password = Credential.Password;
          type3.Username = Credential.UserName;

          clientResponse = ByteString.CreateImmutable(type3.GetBytes());

          step++;

          return SaslExchangeStatus.Succeeded;
        }

        default:
          clientResponse = null;
          return SaslExchangeStatus.Failed; // unexpected server challenge
      }
    }

    private int step;
  }
}

namespace Mono.Security {
  internal static class Locale {
    public static string GetText(string format, params string[] args)
    {
      return string.Format(format, args);
    }
  }
}
