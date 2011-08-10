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

using Smdn.Formats;
using Smdn.Security.Authentication.Sasl;

namespace Smdn.Net.Pop3 {
  // enum types:
  //   PopStringEnum
  //     => handles string constants
  //     PopAuthenticationMechanism
  //       => handles authentication mechanisms
  //   * PopCapability
  //       => handles capability constants

  [Serializable]
  public sealed class PopCapability : PopStringEnum {
    public static readonly PopCapabilitySet AllCapabilities;

    /*
     * POP3 Extension Mechanism
     * http://www.iana.org/assignments/pop3-extension-mechanism
     */
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      Top = new PopCapability("TOP");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      User = new PopCapability("USER");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      Sasl = new PopCapability("SASL");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      RespCodes = new PopCapability("RESP-CODES");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      LoginDelay = new PopCapability("LOGIN-DELAY");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      Pipelining = new PopCapability("PIPELINING");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      Expire = new PopCapability("EXPIRE");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      Uidl = new PopCapability("UIDL");
    public static readonly PopCapability /* [RFC2449] POP3 Extension Mechanism */
                                      Implementation = new PopCapability("IMPLEMENTATION");
    public static readonly PopCapability /* [RFC2595] Using TLS with IMAP, POP3 and ACAP */
                                      Stls = new PopCapability("STLS");
    public static readonly PopCapability /* [RFC3206] The SYS and AUTH POP Response Codes */
                                      AuthRespCode = new PopCapability("AUTH-RESP-CODE");
    public static readonly PopCapability /* [RFC5721] POP3 Support for UTF-8 */
                                      Utf8 = new PopCapability("UTF8");
    public static readonly PopCapability /* [RFC5721] POP3 Support for UTF-8 */
                                      Lang = new PopCapability("LANG");

    /*
     * draft
     */

    /*
     * extended capabilities
     */

    static PopCapability()
    {
      var capabilities = new List<PopCapability>(GetDefinedConstants<PopCapability>());

      foreach (var saslMechansim in SaslMechanisms.AllMechanisms) {
        capabilities.Add(new PopCapability("SASL", saslMechansim));
      }

      AllCapabilities = new PopCapabilitySet(true, capabilities);
    }

    internal static PopCapability GetKnownOrCreate(string capability, string[] arguments)
    {
      PopCapability knownCapability;

      if (AllCapabilities.TryGet(capability, out knownCapability)) {
        if (knownCapability.ContainsAllArguments(arguments))
          return knownCapability;
      }

      //Trace.Verbose("unknown capability: {0} {1}", capability, string.Join(" ", arguments));
      return new PopCapability(capability, arguments);
    }

    /*
     * instance members
     */
    public string Tag {
      get; private set;
    }

    public string[] Arguments {
      get; private set;
    }

    public override string Value {
      get
      {
        if (Arguments.Length == 0)
          return Tag;
        else
          return string.Concat(Tag, " ", string.Join(" ", Arguments));
      }
    }

    public PopCapability(string tag, params string[] arguments)
      : base(tag)
    {
      if (tag == null)
        throw new ArgumentNullException("tag");
      if (tag.Length == 0)
        throw ExceptionUtils.CreateArgumentMustBeNonEmptyString("tag");

      foreach (var arg in arguments) {
        if (string.IsNullOrEmpty(arg))
          throw new ArgumentException("length of argument must be greater than 1", "arguments");
      }

      this.Tag = tag;
      this.Arguments = arguments;
    }

    public bool ContainsAllArguments(params string[] arguments)
    {
      foreach (var argument in arguments) {
        var contains = false;

        foreach (var arg in Arguments) {
          if (string.Equals(argument, arg, StringComparison.OrdinalIgnoreCase)) {
            contains = true;
            break;
          }
        }

        if (!contains)
          return false;
      }

      return true;
    }

    public override string ToString()
    {
      return Value;
    }
  }
}
