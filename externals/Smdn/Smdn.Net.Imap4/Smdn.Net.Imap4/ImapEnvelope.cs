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

namespace Smdn.Net.Imap4 {
  // 7.4.2. FETCH Response
  //       ENVELOPE
  //         The fields of the envelope structure are in the following
  //         order: date, subject, from, sender, reply-to, to, cc, bcc,
  //         in-reply-to, and message-id.  The date, subject, in-reply-to,
  //         and message-id fields are strings.  The from, sender, reply-to,
  //         to, cc, and bcc fields are parenthesized lists of address
  //         structures.
  public sealed class ImapEnvelope {
    public string Date {
      get { return date; }
    }

    public string Subject {
      get { return subject; }
    }

    public ImapAddress[] From {
      get { return @from; }
    }

    public ImapAddress[] Sender {
      get { return sender; }
    }

    public ImapAddress[] ReplyTo {
      get { return replyTo; }
    }

    public ImapAddress[] To {
      get { return @to; }
    }

    public ImapAddress[] Cc {
      get { return cc; }
    }

    public ImapAddress[] Bcc {
      get { return bcc; }
    }

    public string InReplyTo {
      get { return inReplyTo; }
    }

    public string MessageId {
      get { return messageId; }
    }

    public ImapEnvelope(string date,
                        string subject,
                        ImapAddress[] @from,
                        ImapAddress[] sender,
                        ImapAddress[] replyTo,
                        ImapAddress[] @to,
                        ImapAddress[] cc,
                        ImapAddress[] bcc,
                        string inReplyTo,
                        string messageId)
    {
      this.date       = date;
      this.subject    = subject;
      this.@from     = @from;
      this.sender     = sender;
      this.replyTo    = replyTo;
      this.@to        = @to;
      this.cc         = cc;
      this.bcc        = bcc;
      this.inReplyTo  = inReplyTo;
      this.messageId  = messageId;
    }

    public override string ToString()
    {
      return string.Format("{{Date={0}, Subject={1}, From={2}, Sender={3}, ReplyTo={4}, To={5}, Cc={6}, Bcc={7}, InReplyTo={8}, MessageId={9}}}",
                           date,
                           subject,
                           Array.ConvertAll(@from, delegate(ImapAddress address) {return address.ToString();}),
                           Array.ConvertAll(sender, delegate(ImapAddress address) {return address.ToString();}),
                           Array.ConvertAll(replyTo, delegate(ImapAddress address) {return address.ToString();}),
                           Array.ConvertAll(@to, delegate(ImapAddress address) {return address.ToString();}),
                           Array.ConvertAll(cc, delegate(ImapAddress address) {return address.ToString();}),
                           Array.ConvertAll(bcc, delegate(ImapAddress address) {return address.ToString();}),
                           inReplyTo,
                           messageId);
    }

    private /*readonly*/ string date;
    private /*readonly*/ string subject;
    private /*readonly*/ ImapAddress[] @from;
    private /*readonly*/ ImapAddress[] sender;
    private /*readonly*/ ImapAddress[] replyTo;
    private /*readonly*/ ImapAddress[] @to;
    private /*readonly*/ ImapAddress[] cc;
    private /*readonly*/ ImapAddress[] bcc;
    private /*readonly*/ string inReplyTo;
    private /*readonly*/ string messageId;
  }
}
/*
envelope        = "(" env-date SP env-subject SP env-from SP
                  env-sender SP env-reply-to SP env-to SP env-cc SP
                  env-bcc SP env-in-reply-to SP env-message-id ")"
env-bcc         = "(" 1*address ")" / nil
env-cc          = "(" 1*address ")" / nil
env-date        = nstring
env-from        = "(" 1*address ")" / nil
env-in-reply-to = nstring
env-message-id  = nstring
env-reply-to    = "(" 1*address ")" / nil
env-sender      = "(" 1*address ")" / nil
env-subject     = nstring
env-to          = "(" 1*address ")" / nil
*/
