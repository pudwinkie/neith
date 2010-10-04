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

using Smdn.Security.Authentication.Sasl;

namespace Smdn.Net.Pop3 {
  public sealed class PopAuthenticationMechanism : PopStringEnum, IPopExtension {
    public static readonly PopStringEnumList<PopAuthenticationMechanism> AllMechanisms;

    /*
     * RFC 2384 POP URL Scheme
     * http://tools.ietf.org/html/rfc2384
     * 4. POP User Name and Authentication Mechanism
     *    An authentication mechanism can be expressed by adding ";AUTH=<enc-
     *    auth-type>" to the end of the user name.  If the authentication
     *    mechanism name is not preceded by a "+", it is a SASL POP [SASL]
     *    mechanism.  If it is preceded by a "+", it is either "APOP" or an
     *    extension mechanism.
     */

    /*
     * APOP or extension mechanisms
     */
    public static readonly PopAuthenticationMechanism Apop
      = new PopAuthenticationMechanism("+APOP");

    /*
     * SASL POP mechanisms
     */
    public static readonly PopAuthenticationMechanism KerberosV4
                 = CreateSaslMechanism(SaslMechanisms.KerberosV4);
    public static readonly PopAuthenticationMechanism CRAMMD5
                 = CreateSaslMechanism(SaslMechanisms.CRAMMD5);
    public static readonly PopAuthenticationMechanism OTP
                 = CreateSaslMechanism(SaslMechanisms.OTP);
    public static readonly PopAuthenticationMechanism SKey
                 = CreateSaslMechanism(SaslMechanisms.SKey);
    public static readonly PopAuthenticationMechanism Plain
                 = CreateSaslMechanism(SaslMechanisms.Plain);
    public static readonly PopAuthenticationMechanism SecureID
                 = CreateSaslMechanism(SaslMechanisms.SecureID);
    public static readonly PopAuthenticationMechanism DigestMD5
                 = CreateSaslMechanism(SaslMechanisms.DigestMD5);
    public static readonly PopAuthenticationMechanism IsoIec9798_U_RSA_SHA1_ENC
                 = CreateSaslMechanism(SaslMechanisms.IsoIec9798_U_RSA_SHA1_ENC);
    public static readonly PopAuthenticationMechanism IsoIec9798_M_RSA_SHA1_ENC
                 = CreateSaslMechanism(SaslMechanisms.IsoIec9798_M_RSA_SHA1_ENC);
    public static readonly PopAuthenticationMechanism IsoIec9798_U_DSA_SHA1
                 = CreateSaslMechanism(SaslMechanisms.IsoIec9798_U_DSA_SHA1);
    public static readonly PopAuthenticationMechanism IsoIec9798_M_DSA_SHA1
                 = CreateSaslMechanism(SaslMechanisms.IsoIec9798_M_DSA_SHA1);
    public static readonly PopAuthenticationMechanism IsoIec9798_U_ECDSA_SHA1
                 = CreateSaslMechanism(SaslMechanisms.IsoIec9798_U_ECDSA_SHA1);
    public static readonly PopAuthenticationMechanism IsoIec9798_M_ECDSA_SHA1
                 = CreateSaslMechanism(SaslMechanisms.IsoIec9798_M_ECDSA_SHA1);
    public static readonly PopAuthenticationMechanism External
                 = CreateSaslMechanism(SaslMechanisms.External);
    public static readonly PopAuthenticationMechanism Anonymous
                 = CreateSaslMechanism(SaslMechanisms.Anonymous);
    public static readonly PopAuthenticationMechanism Gssapi
                 = CreateSaslMechanism(SaslMechanisms.Gssapi);
    public static readonly PopAuthenticationMechanism KerberosV5
                 = CreateSaslMechanism(SaslMechanisms.KerberosV5);
    public static readonly PopAuthenticationMechanism NTLM
                 = CreateSaslMechanism(SaslMechanisms.NTLM);

    public static readonly PopAuthenticationMechanism Login
                 = CreateSaslMechanism(SaslMechanisms.Login);

    /*
     * RFC 2384 POP URL Scheme
     * http://tools.ietf.org/html/rfc2384
     */
    public static readonly PopAuthenticationMechanism SelectAppropriate = new PopAuthenticationMechanism("*");

    static PopAuthenticationMechanism()
    {
      AllMechanisms = CreateDefinedConstantsList<PopAuthenticationMechanism>();
    }

    private static PopAuthenticationMechanism CreateSaslMechanism(string saslMechanismName)
    {
      return new PopAuthenticationMechanism(saslMechanismName,
                                            PopCapability.GetKnownOrCreate("SASL", new[] {saslMechanismName}));
    }

    public static PopAuthenticationMechanism GetKnownOrCreate(string mechanismName)
    {
      if (AllMechanisms.Has(mechanismName))
        return AllMechanisms[mechanismName];
      else
        //Trace.Verbose("unknown authentication mechanism: {0}", mechanism);
        return new PopAuthenticationMechanism(mechanismName);
    }

    public PopCapability RequiredCapability {
      get; private set;
    }

    public PopAuthenticationMechanism(string mechanismName)
      : this(mechanismName, null)
    {
    }

    public PopAuthenticationMechanism(string mechanismName, PopCapability requiredCapability)
      : base(mechanismName)
    {
      if (mechanismName.Length == 0)
        throw new ArgumentException("invalid name", "mechanismName");

      this.RequiredCapability = requiredCapability;
    }
  }
}