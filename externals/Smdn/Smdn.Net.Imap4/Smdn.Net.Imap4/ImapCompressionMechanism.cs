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

namespace Smdn.Net.Imap4 {
  // enum types:
  //   ImapStringEnum : ImapString
  //     => handles string constants
  //     ImapAuthenticationMechanism
  //       => handles authentication mechanisms
  //     ImapCapability
  //       => handles capability constants
  //   * ImapCompressionMechanism
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

  // http://tools.ietf.org/html/rfc4978
  // 3. The COMPRESS Command
  public sealed class ImapCompressionMechanism : ImapStringEnum, IImapExtension {
    public static readonly ImapStringEnumList<ImapCompressionMechanism> AllMechanisms;

    //    The DEFLATE algorithm (defined in [RFC1951]) is
    //    standard, widely available and fairly efficient, so it is the only
    //    algorithm defined by this document.
    public static readonly ImapCompressionMechanism Deflate = new ImapCompressionMechanism("DEFLATE", ImapCapability.CompressDeflate);

    static ImapCompressionMechanism()
    {
      AllMechanisms = CreateDefinedConstantsList<ImapCompressionMechanism>();
    }

    ImapCapability IImapExtension.RequiredCapability {
      get { return requiredCapability; }
    }

    private ImapCompressionMechanism(string compressionMechanism, ImapCapability requiredCapability)
      : base(compressionMechanism)
    {
      this.requiredCapability = requiredCapability;
    }

    private /*readonly*/ ImapCapability requiredCapability;
  }
}