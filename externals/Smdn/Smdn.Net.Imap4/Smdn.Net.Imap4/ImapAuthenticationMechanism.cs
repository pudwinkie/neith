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

using Smdn.Security.Authentication.Sasl;

namespace Smdn.Net.Imap4 {
  // enum types:
  //   ImapStringEnum : ImapString
  //     => handles string constants
  //   * ImapAuthenticationMechanism
  //       => handles authentication mechanisms
  //     ImapCapability
  //       => handles capability constants
  //     ImapCompressionMechanism
  //       => handles COMPRESS extension compression mechanism
  //     ImapMailboxFlag
  //       => handles mailbox flags
  //     ImapMessageFlag
  //       => handles system flags and keywords
  //     ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  public sealed class ImapAuthenticationMechanism : ImapStringEnum, IImapExtension {
    public static readonly ImapStringEnumList<ImapAuthenticationMechanism> AllMechanisms;

    public static readonly ImapAuthenticationMechanism KerberosV4
                  = CreateSaslMechanism(SaslMechanisms.KerberosV4);
    public static readonly ImapAuthenticationMechanism CRAMMD5
                  = CreateSaslMechanism(SaslMechanisms.CRAMMD5);
    public static readonly ImapAuthenticationMechanism OTP
                  = CreateSaslMechanism(SaslMechanisms.OTP);
    public static readonly ImapAuthenticationMechanism SKey
                  = CreateSaslMechanism(SaslMechanisms.SKey);
    public static readonly ImapAuthenticationMechanism Plain
                  = CreateSaslMechanism(SaslMechanisms.Plain);
    public static readonly ImapAuthenticationMechanism SecureID
                  = CreateSaslMechanism(SaslMechanisms.SecureID);
    public static readonly ImapAuthenticationMechanism DigestMD5
                  = CreateSaslMechanism(SaslMechanisms.DigestMD5);
    public static readonly ImapAuthenticationMechanism IsoIec9798_U_RSA_SHA1_ENC
                  = CreateSaslMechanism(SaslMechanisms.IsoIec9798_U_RSA_SHA1_ENC);
    public static readonly ImapAuthenticationMechanism IsoIec9798_M_RSA_SHA1_ENC
                  = CreateSaslMechanism(SaslMechanisms.IsoIec9798_M_RSA_SHA1_ENC);
    public static readonly ImapAuthenticationMechanism IsoIec9798_U_DSA_SHA1
                  = CreateSaslMechanism(SaslMechanisms.IsoIec9798_U_DSA_SHA1);
    public static readonly ImapAuthenticationMechanism IsoIec9798_M_DSA_SHA1
                  = CreateSaslMechanism(SaslMechanisms.IsoIec9798_M_DSA_SHA1);
    public static readonly ImapAuthenticationMechanism IsoIec9798_U_ECDSA_SHA1
                  = CreateSaslMechanism(SaslMechanisms.IsoIec9798_U_ECDSA_SHA1);
    public static readonly ImapAuthenticationMechanism IsoIec9798_M_ECDSA_SHA1
                  = CreateSaslMechanism(SaslMechanisms.IsoIec9798_M_ECDSA_SHA1);
    public static readonly ImapAuthenticationMechanism External
                  = CreateSaslMechanism(SaslMechanisms.External);
    public static readonly ImapAuthenticationMechanism Anonymous
                  = CreateSaslMechanism(SaslMechanisms.Anonymous);
    public static readonly ImapAuthenticationMechanism Gssapi
                  = CreateSaslMechanism(SaslMechanisms.Gssapi);
    public static readonly ImapAuthenticationMechanism KerberosV5
                  = CreateSaslMechanism(SaslMechanisms.KerberosV5);
    public static readonly ImapAuthenticationMechanism NTLM
                  = CreateSaslMechanism(SaslMechanisms.NTLM);

    public static readonly ImapAuthenticationMechanism Login
                  = CreateSaslMechanism(SaslMechanisms.Login);

    /*
     * RFC 5092 IMAP URL Scheme
     * http://tools.ietf.org/html/rfc5092
     */
    public static readonly ImapAuthenticationMechanism SelectAppropriate = new ImapAuthenticationMechanism("*");

    static ImapAuthenticationMechanism()
    {
      AllMechanisms = CreateDefinedConstantsList<ImapAuthenticationMechanism>();
    }

    private static ImapAuthenticationMechanism CreateSaslMechanism(string saslMechanismName)
    {
      return new ImapAuthenticationMechanism(saslMechanismName,
                                             ImapCapability.GetKnownOrCreate("AUTH=" + saslMechanismName));
    }

    public static ImapAuthenticationMechanism GetKnownOrCreate(string mechanismName)
    {
      if (AllMechanisms.Has(mechanismName))
        return AllMechanisms[mechanismName];
      else
        //Trace.Verbose("unknown authentication mechanism: {0}", mechanismName);
        return new ImapAuthenticationMechanism(mechanismName);
    }

    public ImapCapability RequiredCapability {
      get; private set;
    }

    public ImapAuthenticationMechanism(string mechanismName)
      : this(mechanismName, null)
    {
    }

    public ImapAuthenticationMechanism(string mechanismName, ImapCapability requiredCapability)
      : base(mechanismName)
    {
      if (mechanismName.Length == 0)
        throw new ArgumentException("invalid name", "mechanismName");

      this.RequiredCapability = requiredCapability;
    }
  }
}