// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
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

using Smdn.Formats;
using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Protocol.Client {
  public static class PopResponseConverter {
    public static PopDropListing FromStat(PopStatusResponse response)
    {
      /*
       * 5. The TRANSACTION State
       * STAT
       *    In order to simplify parsing, all POP3 servers are
       *    required to use a certain format for drop listings.  The
       *    positive response consists of "+OK" followed by a single
       *    space, the number of messages in the maildrop, a single
       *    space, and the size of the maildrop in octets.  This memo
       *    makes no requirement on what follows the maildrop size.
       *    Minimal implementations should just end that line of the
       *    response with a CRLF pair.  More advanced implementations
       *    may include other information.
       */
      return PopTextConverter.ToDropListing(SplitDataOrThrow(response, 2));
    }

    public static PopCapability FromCapa(PopFollowingResponse response)
    {
      /*
       * http://tools.ietf.org/html/rfc2449
       * RFC 2449 - POP3 Extension Mechanism
       * 
       * 5. The CAPA Command
       * 
       *    An -ERR response indicates the capability command is not
       *    implemented and the client will have to probe for
       *    capabilities as before.
       * 
       *    An +OK response is followed by a list of capabilities, one
       *    per line.  Each capability name MAY be followed by a single
       *    space and a space-separated list of parameters.  Each
       *    capability line is limited to 512 octets (including the
       *    CRLF).  The capability list is terminated by a line
       *    containing a termination octet (".") and a CRLF pair.
       */
      return PopTextConverter.ToCapability(SplitDataOrThrow(response, 1));
    }

    /*
     * 5. The TRANSACTION State
     * LIST
     *    In order to simplify parsing, all POP3 servers are
     *    required to use a certain format for scan listings.  A
     *    scan listing consists of the message-number of the
     *    message, followed by a single space and the exact size of
     *    the message in octets.  Methods for calculating the exact
     *    size of the message are described in the "Message Format"
     *    section below.  This memo makes no requirement on what
     *    follows the message size in the scan listing.  Minimal
     *    implementations should just end that line of the response
     *    with a CRLF pair.  More advanced implementations may
     *    include other information, as parsed from the message.
     */
    public static PopScanListing FromList(PopStatusResponse response)
    {
      /*
       * 5. The TRANSACTION State
       * LIST
       *    If an argument was given and the POP3 server issues a
       *    positive response with a line containing information for
       *    that message.  This line is called a "scan listing" for
       *    that message.
       */
      return PopTextConverter.ToScanListing(SplitDataOrThrow(response, 2));
    }

    public static PopScanListing FromList(PopFollowingResponse response)
    {
      /*
       * 5. The TRANSACTION State
       * LIST
       *    If no argument was given and the POP3 server issues a
       *    positive response, then the response given is multi-line.
       *    After the initial +OK, for each message in the maildrop,
       *    the POP3 server responds with a line containing
       *    information for that message.  This line is also called a
       *    "scan listing" for that message.  If there are no
       *    messages in the maildrop, then the POP3 server responds
       *    with no scan listings--it issues a positive response
       *    followed by a line containing a termination octet and a
       *    CRLF pair.
       */
      return PopTextConverter.ToScanListing(SplitDataOrThrow(response, 2));
    }

    /*
     * 7. Optional POP3 Commands
     * UIDL
     *    In order to simplify parsing, all POP3 servers are required to
     *    use a certain format for unique-id listings.  A unique-id
     *    listing consists of the message-number of the message,
     *    followed by a single space and the unique-id of the message.
     *    No information follows the unique-id in the unique-id listing.
     */
    public static PopUniqueIdListing FromUidl(PopStatusResponse response)
    {
      /*
       * 7. Optional POP3 Commands
       * UIDL
       *    If an argument was given and the POP3 server issues a positive
       *    response with a line containing information for that message.
       *    This line is called a "unique-id listing" for that message.
       */
      return PopTextConverter.ToUniqueIdListing(SplitDataOrThrow(response, 2));
    }

    public static PopUniqueIdListing FromUidl(PopFollowingResponse response)
    {
      /*
       * 7. Optional POP3 Commands
       * UIDL
       *    If no argument was given and the POP3 server issues a positive
       *    response, then the response given is multi-line.  After the
       *    initial +OK, for each message in the maildrop, the POP3 server
       *    responds with a line containing information for that message.
       *    This line is called a "unique-id listing" for that message.
       */
      return PopTextConverter.ToUniqueIdListing(SplitDataOrThrow(response, 2));
    }

    public static string FromGreetingBanner(PopStatusResponse response)
    {
      /*
       * 7. Optional POP3 Commands
       * APOP
       *    A POP3 server which implements the APOP command will
       *    include a timestamp in its banner greeting.  The syntax of
       *    the timestamp corresponds to the `msg-id' in [RFC822]
       */
      var start = response.Text.IndexOf('<');

      if (start < 0)
        return null;

      var end = response.Text.IndexOf('>', start);

      if (end <= start)
        throw new PopMalformedResponseException("invalid banner greeting");

      return response.Text.Substring(start, end - start + 1);
    }

    private static ByteString[] SplitDataOrThrow(IPopDataResponse response, int expectedTextCount)
    {
      var texts = response.Data == null
        ? new[] {ByteString.CreateEmpty()}
        : response.Data.Split(Octets.SP);

      if (texts.Length < expectedTextCount)
        throw new PopMalformedResponseException(string.Format("too few text counts; expected is {0} but was {1}", expectedTextCount, texts.Length));

      return texts;
    }
  }
}
