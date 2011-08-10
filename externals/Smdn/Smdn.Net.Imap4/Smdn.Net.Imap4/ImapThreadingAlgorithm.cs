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

namespace Smdn.Net.Imap4 {
  // enum types:
  //   ImapStringEnum : ImapString
  //     => handles string constants
  //     ImapAuthenticationMechanism
  //       => handles authentication mechanisms
  //     ImapCapability
  //       => handles capability constants
  //     ImapCompressionMechanism
  //       => handles COMPRESS extension compression mechanism
  //     ImapMailboxFlag
  //       => handles mailbox flags
  //     ImapMessageFlag
  //       => handles system flags and keywords
  //   * ImapThreadingAlgorithm
  //       => handles THREAD extension threading algorithm
  //     Protocol.ImapResponseCode
  //       => handles response codes
  //     Protocol.ImapDataResponseType
  //       => handles server response types

  // http://tools.ietf.org/html/rfc5256
  // BASE.6.4.THREAD. THREAD Command
  public sealed class ImapThreadingAlgorithm : ImapStringEnum, IImapExtension {
    public static readonly ImapStringEnumSet<ImapThreadingAlgorithm> AllAlgorithms;

    // ORDEREDSUBJECT
    //    The ORDEREDSUBJECT threading algorithm is also referred to as
    //    "poor man's threading".  The searched messages are sorted by
    //    base subject and then by the sent date.  The messages are then
    //    split into separate threads, with each thread containing
    //    messages with the same base subject text.  Finally, the threads
    //    are sorted by the sent date of the first message in the thread.
    public static readonly ImapThreadingAlgorithm OrderedSubject = new ImapThreadingAlgorithm("ORDEREDSUBJECT", ImapCapability.ThreadOrderedSubject);

    // REFERENCES
    //    The REFERENCES threading algorithm threads the searched
    //    messages by grouping them together in parent/child
    //    relationships based on which messages are replies to others.
    //    The parent/child relationships are built using two methods:
    //    reconstructing a message's ancestry using the references
    //    contained within it; and checking the original (not base)
    //    subject of a message to see if it is a reply to (or forward of)
    //    another message.
    public static readonly ImapThreadingAlgorithm References = new ImapThreadingAlgorithm("REFERENCES", ImapCapability.ThreadReferences);

    /*
     * The IMAP SEARCH=INTHREAD and THREAD=REFS Extensions
     * http://tools.ietf.org/html/draft-ietf-morg-inthread-00
     */

    // 4. The THREAD=REFS Thread Algorithm
    // 
    //     The THREAD=REFS thread algorithm is defined as the part of
    //     THREAD=REFERENCES (see [RFC5256]) which concerns itself with the
    //     References, In-Reply-To and Message-ID fields.  THREAD=REFS ignores
    //     Subject.
    // 
    //     THREAD=REFS sorts threads by the most recent INTERNALDATE in each
    //     thread, replacing THREAD=REFERENCES step (4). This means that when a
    //     new message arrives, its thread becomes the latest thread. (Note
    //     that while threads are sorted by arrival date, messages within a
    //     thread are sorted by sent date, just as for THREAD=REFERENCES.)
    public static readonly ImapThreadingAlgorithm Refs = new ImapThreadingAlgorithm("REFS", ImapCapability.ThreadRefs);

    static ImapThreadingAlgorithm()
    {
      AllAlgorithms = CreateDefinedConstantsSet<ImapThreadingAlgorithm>();
    }

    IEnumerable<ImapCapability> IImapExtension.RequiredCapabilities {
      get
      {
        if (requiredCapability != null)
          yield return requiredCapability;
      }
    }

    private ImapThreadingAlgorithm(string threadingAlgorithm, ImapCapability requiredCapability)
      : base(threadingAlgorithm)
    {
      this.requiredCapability = requiredCapability;
    }

    private readonly ImapCapability requiredCapability;
  }
}