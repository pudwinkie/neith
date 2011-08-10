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
using System.IO;
using System.Text;

using Smdn.Formats;

namespace Smdn.Net.Imap4.Protocol.Client {
  public sealed class ImapResponseReceiver : ImapReceiver {
    public bool ResponseContinuing {
      get { return dataParsingContext != null; }
    }

    public bool ReceiveResponseAsUTF8 {
      get; set;
    }

    public ImapResponseReceiver(LineOrientedBufferedStream stream)
      : base(stream)
    {
      ReceiveResponseAsUTF8 = false;
    }

    private IParsingContext dataParsingContext;

    public ImapResponse ReceiveResponse()
    {
      var line = Receive();

      try {
        return (dataParsingContext == null)
          ? ParseResponce(line, ref dataParsingContext)
          : ParseDataResponse(line, ref dataParsingContext);
      }
      catch (Exception ex) {
        dataParsingContext = null; // discard context
        if (ex is ImapMalformedDataException)
          throw new ImapMalformedResponseException(ex.Message);
        else
          throw;
      }
    }

    private readonly static ByteString continueReqMark = ByteString.CreateImmutable("+ ");
    private readonly static ByteString respCondMark = ByteString.CreateImmutable("* [");

    private ImapResponse ParseResponce(ByteString line, ref IParsingContext parsingContext)
    {
      // response        = *(continue-req / response-data) response-done

      // continue-req    = "+" SP (resp-text / base64) CRLF
      if (line.StartsWith(continueReqMark))
        return new ImapCommandContinuationRequest(line.ToString(2, line.Length - 4)); // remove leading "+" SP and trailing CRLF

      // greeting        = "*" SP (resp-cond-auth / resp-cond-bye) CRLF
      // response-done   = response-tagged / response-fatal
      // response-data   = "*" SP (resp-cond-state / resp-cond-bye /
      //                   mailbox-data / message-data / capability-data) CRLF
      // response-fatal  = "*" SP resp-cond-bye CRLF
      //                     ; Server closes connection immediately
      // response-tagged = tag SP resp-cond-state CRLF

      // ("*" / tag) SP
      var tagSep = line.IndexOf(Octets.SP);

      if (tagSep == -1)
        // response-done and response-data must contain SP
        throw new ImapMalformedResponseException("malformed response-done/response-data", line.ToString());

      var untagged = (tagSep == 1 && line[0] == ImapOctets.Asterisk);

      // ("OK" / "BAD" / "NO" / "BYE" / "PREAUTH" / text) SP
      var respCondSep = line.IndexOf(Octets.SP, tagSep + 1);
      var cond = ImapResponseCondition.Undefined;

      if (respCondSep == -1) {
        if (!untagged)
          throw new ImapMalformedResponseException("malformed response-data", line.ToString());
        //else
          // '* SEARCH\r\n' (mailbox-data which contains no SP)
      }
      else {
        cond = ParseCondition(line.Substring(tagSep + 1, respCondSep - tagSep - 1));
      }

      if (cond != ImapResponseCondition.Undefined || line.StartsWith(respCondMark)) {
        // resp-cond-auth / resp-cond-state / resp-cond-bye
        var responseText = ParseRespText((cond == ImapResponseCondition.Undefined)
                                           ? line.Substring(tagSep + 1)
                                           : line.Substring(respCondSep + 1));

        if (untagged)
          return new ImapUntaggedStatusResponse((ImapResponseCondition)cond,
                                            responseText);
        else
          return new ImapTaggedStatusResponse(line.ToString(0, tagSep),
                                          (ImapResponseCondition)cond,
                                          responseText);
      }

      // mailbox-data / message-data / capability-data etc.
      return ParseDataResponse(line, ref parsingContext);
    }

    private static readonly ByteString respCondOk      = ByteString.CreateImmutable("OK");
    private static readonly ByteString respCondNo      = ByteString.CreateImmutable("NO");
    private static readonly ByteString respCondBad     = ByteString.CreateImmutable("BAD");
    private static readonly ByteString respCondBye     = ByteString.CreateImmutable("BYE");
    private static readonly ByteString respCondPreAuth = ByteString.CreateImmutable("PREAUTH");
    
    private ImapResponseCondition ParseCondition(ByteString cond)
    {
      if (respCondOk.EqualsIgnoreCase(cond))
        return ImapResponseCondition.Ok;
      else if (respCondNo.EqualsIgnoreCase(cond))
        return ImapResponseCondition.No;
      else if (respCondBye.EqualsIgnoreCase(cond))
        return ImapResponseCondition.Bye;
      else if (respCondBad.EqualsIgnoreCase(cond))
        return ImapResponseCondition.Bad;
      else if (respCondPreAuth.EqualsIgnoreCase(cond))
        return ImapResponseCondition.PreAuth;
      else
        return ImapResponseCondition.Undefined;
    }

    private ImapResponseText ParseRespText(ByteString respText)
    {
      // resp-text       = ["[" resp-text-code "]" SP] text
      if (respText[0] != ImapOctets.OpenBracket)
        // no resp-text-code
        return new ImapResponseText(DecodeRespTextString(respText.Substring(0, respText.Length - 2))); // remove trailing CRLF

      // exists resp-text-code
      var respTextCodeEnd = respText.IndexOf(ImapOctets.CloseBracket);
      var respTextCode    = respText.Substring(1, respTextCodeEnd - 1);
      var codeSep         = respTextCode.IndexOf(Octets.SP);

      ByteString text;

      if (respTextCodeEnd < respText.Length) {
        text = respText.Substring(respTextCodeEnd + 1);
        text = text.Trim(); // remove SP, CRLF
      }
      else {
        text = ByteString.CreateEmpty();
      }

      if (codeSep == -1)
        // no arguments; "READ-ONLY" / "READ-WRITE" / etc...
        return new ImapResponseText(ImapResponseCode.GetKnownOrCreate(respTextCode.ToString()),
                                    new ImapData[] {},
                                    DecodeRespTextString(text));

      // exist arguments; "UIDNEXT" SP nz-number / "UIDVALIDITY" SP nz-number / etc...
      var codeString = respTextCode.Substring(0, codeSep);
      var arguments = ParseDataNonTerminatedText(respTextCode, codeSep + 1);

      if (arguments == null)
        throw new ImapMalformedResponseException("malformed resp-text-code", respTextCode.ToString(codeSep + 1));

      return new ImapResponseText(ImapResponseCode.GetKnownOrCreate(codeString.ToString(), arguments[0]),
                                  arguments,
                                  DecodeRespTextString(text));
    }

    private string DecodeRespTextString(ByteString text)
    {
      /*
       * RFC 5255 - Internet Message Access Protocol Internationalization
       * http://tools.ietf.org/html/rfc5255
       * 3.5. Formal Syntax
       *     resp-text         = ["[" resp-text-code "]" SP ] UTF8-TEXT-CHAR
       *                         *(UTF8-TEXT-CHAR / "[")
       *         ; After the server is changed to a language other than
       *         ; i-default, this resp-text rule replaces the resp-text
       *         ; rule from [RFC3501].
       */
      if (ReceiveResponseAsUTF8)
        return text.ToString(Encoding.UTF8);
      else
        return text.ToString();
    }

    private ImapDataResponse ParseDataResponse(ByteString line, ref IParsingContext parsingContext)
    {
      // mailbox-data / message-data / capability-data etc.

      // ("*" / tag) SP
      var offset = (parsingContext == null) ? line.IndexOf(Octets.SP) + 1 : 0;
      var parsedData = ParseData(line, offset, ref parsingContext);

      if (parsedData == null)
        return null;
      else
        return ImapDataResponse.Create(parsedData);
    }
  }
}