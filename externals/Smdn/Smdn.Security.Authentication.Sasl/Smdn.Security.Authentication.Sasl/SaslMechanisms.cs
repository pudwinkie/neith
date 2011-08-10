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
using System.Collections.Generic;
using System.Reflection;

namespace Smdn.Security.Authentication.Sasl {
  /*
   * SIMPLE AUTHENTICATION AND SECURITY LAYER (SASL) MECHANISMS
   * http://www.iana.org/assignments/sasl-mechanisms
   */
  public static class SaslMechanisms {
    public static readonly IEnumerable<string> AllMechanisms;

    /* [RFC1731] IMAP4 Authentication Mechanisms */
    public const string KerberosV4 = "KERBEROS_V4";
    /* [RFC2195] IMAP/POP AUTHorize Extension for Simple Challenge/Response */
    public const string CRAMMD5 = "CRAM-MD5";
    /* [RFC2444] The One-Time-Password SASL Mechanism */
    public const string OTP = "OTP";
    /* [RFC2444] The One-Time-Password SASL Mechanism */
    public const string SKey = "SKEY";
    /* [RFC2595] Using TLS with IMAP, POP3 and ACAP */
    public const string Plain = "PLAIN";
    /* [RFC2808] The SecurID(r) SASL Mechanism */
    public const string SecureID = "SECURID";
    /* [RFC2831] Using Digest Authentication as a SASL Mechanism */
    public const string DigestMD5 = "DIGEST-MD5";
    /* [RFC3163] ISO/IEC 9798-3 Authentication SASL Mechanism */
    public const string IsoIec9798_U_RSA_SHA1_ENC = "9798-U-RSA-SHA1-ENC";
    /* [RFC3163] ISO/IEC 9798-3 Authentication SASL Mechanism */
    public const string IsoIec9798_M_RSA_SHA1_ENC = "9798-M-RSA-SHA1-ENC";
    /* [RFC3163] ISO/IEC 9798-3 Authentication SASL Mechanism */
    public const string IsoIec9798_U_DSA_SHA1 = "9798-U-DSA-SHA1";
    /* [RFC3163] ISO/IEC 9798-3 Authentication SASL Mechanism */
    public const string IsoIec9798_M_DSA_SHA1 = "9798-M-DSA-SHA1";
    /* [RFC3163] ISO/IEC 9798-3 Authentication SASL Mechanism */
    public const string IsoIec9798_U_ECDSA_SHA1 = "9798-U-ECDSA-SHA1";
    /* [RFC3163] ISO/IEC 9798-3 Authentication SASL Mechanism */
    public const string IsoIec9798_M_ECDSA_SHA1 = "9798-M-ECDSA-SHA1";
    /* [RFC4422] Simple Authentication and Security Layer (SASL) */
    public const string External = "EXTERNAL";
    /* [RFC4505] Anonymous Simple Authentication and Security Layer (SASL) Mechanism */
    public const string Anonymous = "ANONYMOUS";
    /* [RFC4752] The Kerberos V5 ("GSSAPI") SASL mechanisma */
    public const string Gssapi = "GSSAPI";

    /* */
    public const string KerberosV5 = "KERBEROS_V5";

    /* Microsoft NTLM Challenge/Response authentication method */
    public const string NTLM = "NTLM";

    /* ? */
    public const string Login = "LOGIN";

    static SaslMechanisms()
    {
      var allMechanisms = new List<string>();

      foreach (var field in typeof(SaslMechanisms).GetFields(BindingFlags.Public | BindingFlags.Static)) {
        if (!field.IsLiteral)
          continue;
        else if (field.FieldType != typeof(string))
          continue;
        else
          allMechanisms.Add((string)field.GetRawConstantValue());
      }

      AllMechanisms = allMechanisms.AsReadOnly();
    }
  }
}