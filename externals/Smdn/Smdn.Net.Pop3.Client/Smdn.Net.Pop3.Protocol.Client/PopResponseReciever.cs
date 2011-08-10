// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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
using System.IO;
using System.Text;

using Smdn.Formats;

namespace Smdn.Net.Pop3.Protocol.Client {
  public sealed class PopResponseReceiver : PopReceiver {
    public bool HandleAsMultiline {
      get; set;
    }

    public PopResponseReceiver(LineOrientedBufferedStream stream)
      : base(stream)
    {
      HandleAsMultiline = false;
    }

    public bool ReceiveLine(Stream stream)
    {
      var line = Receive();

      if (TerminationMark.Equals(line))
        return false;
      else if (line[0] == PopOctets.Period)
        // byte-stutted
        stream.Write(line, 1, line.Length - 1);
      else
        stream.Write(line, 0, line.Length);

      return true;
    }

    public PopResponse ReceiveResponse()
    {
      try {
        return ParseResponce(Receive());
      }
      catch (Exception ex) {
        if (ex is PopMalformedTextException)
          throw new PopMalformedResponseException(ex.Message);
        else
          throw;
      }
    }

    private static readonly ByteString PositiveStatusIndicator = ByteString.CreateImmutable("+OK");
    private static readonly ByteString NegativeStatusIndicator = ByteString.CreateImmutable("-ERR");
    private static readonly ByteString ContinueRequestMark = ByteString.CreateImmutable((byte)'+', Octets.SP);
    private static readonly ByteString ContinueRequestLine = ByteString.CreateImmutable((byte)'+', Octets.CR, Octets.LF);
    private static readonly ByteString TerminationMark = ByteString.CreateImmutable(PopOctets.Period, Octets.CR, Octets.LF);

    private PopResponse ParseResponce(byte[] line)
    {
      if (TerminationMark.Equals(line))
        return new PopTerminationResponse();

      if (HandleAsMultiline) {
        if (line[0] == PopOctets.Period)
          // byte-stutted
          return new PopFollowingResponse(ByteString.CreateImmutable(line, 1, line.Length - 3/*'.' + CRLF*/));
        else
          return new PopFollowingResponse(ByteString.CreateImmutable(line, 0, line.Length - 2/*CRLF*/));
      }
      else {
        // There are currently two status
        // indicators: positive ("+OK") and negative ("-ERR").  Servers MUST
        // send the "+OK" and "-ERR" in upper case.
        if (PositiveStatusIndicator.IsPrefixOf(line))
          return new PopStatusResponse(PopStatusIndicator.Positive,
                                       ParseText(line, PositiveStatusIndicator.Length));
        else if (NegativeStatusIndicator.IsPrefixOf(line))
          return new PopStatusResponse(PopStatusIndicator.Negative,
                                       ParseText(line, NegativeStatusIndicator.Length));
        else if (ContinueRequestMark.IsPrefixOf(line))
          // continue-req     = "+" SP [base64] CRLF
          return new PopContinuationRequest(ByteString.CreateImmutable(line, 2, line.Length - 4/*+, SP, CRLF*/));
        else if (ContinueRequestLine.Equals(line))
          // XXX: dovecot specific? '+CRLF'
          return new PopContinuationRequest(ByteString.CreateEmpty());
        else
          return new PopFollowingResponse(ByteString.CreateImmutable(line, 0, line.Length - 2/*CRLF*/));
      }
    }

    private PopResponseText ParseText(byte[] line, int posStatusEnd)
    {
      /*
       * http://tools.ietf.org/html/rfc2449
       * 3. General Command and Response Grammar
       *       single-line  =  status [SP text] CRLF       ;512 octets maximum
       *       status       =  "+OK" / "-ERR"
       *       text         =  *schar / resp-code *CHAR
       *       resp-code    =  "[" resp-level *("/" resp-level) "]"
       *       resp-level   =  1*rchar
       *       schar        =  %x21-5A / %x5C-7F
       *                           ;printable ASCII, excluding "["
       *       rchar        =  %x21-2E / %x30-5C / %x5E-7F
       *                           ;printable ASCII, excluding "/" and "]"
       */

      /*
       * RFC 1957 - Some Observations on Implementations of the Post Office Protocol (POP3)
       * http://tools.ietf.org/html/rfc1957
       *    Sometimes an implementation is mistaken for a standard.  POP3 servers
       *    and clients are no exception.  The widely-used UCB POP3 server,
       *    popper, which has been further developed by Qualcomm, always has
       *    additional information following the status indicator.  So, the
       *    status indicator always has a space following it.  Two POP3 clients
       *    have been observed to expect that space, and fail when it has not
       *    been found.  The RFC does not require the space, hence this memo.
       */
      var len = line.Length - 2/*CRLF*/;

      if (len == posStatusEnd)
        return new PopResponseText(); // 'status CRLF'

      posStatusEnd++; // SP

      if (len == posStatusEnd)
        return new PopResponseText(); // 'status SP CRLF'

      var text = ByteString.CreateImmutable(line, posStatusEnd, len - posStatusEnd);

      if (text[0] != PopOctets.OpenBracket)
        return new PopResponseText(null, text);

      var respCodeEnd = text.IndexOf(PopOctets.CloseBracket);

      if (respCodeEnd < 0)
        return new PopResponseText(null, text);
      else
        return new PopResponseText(PopResponseCode.GetKnownOrCreate(text.ToString(1/*'['*/, respCodeEnd - 1)),
                                   text.Substring(respCodeEnd + 1).TrimStart());
    }
  }
}
