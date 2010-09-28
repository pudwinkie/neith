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
using System.Net.Mail;
using System.Text;

using Smdn.Formats.Mime;

namespace Smdn.Net.Imap4 {
  // 7.4.2. FETCH Response
  //       ENVELOPE
  //         An address structure is a parenthesized list that describes an
  //         electronic mail address.  The fields of an address structure
  //         are in the following order: personal name, [SMTP]
  //         at-domain-list (source route), mailbox name, and host name.
  public sealed class ImapAddress {
    public string Name {
      get { return name; }
    }

    public string AtDomailList {
      get { return atDomainList; }
    }

    public string Mailbox {
      get { return mailbox; }
    }

    public string Host {
      get { return host; }
    }

    public ImapAddress(string name, string atDomainList, string mailbox, string host)
    {
      this.name = name;
      this.atDomainList = atDomainList;
      this.mailbox = mailbox;
      this.host = host;
    }

    public MailAddress ToMailAddress()
    {
      var address = string.Concat(mailbox, "@", host);

      if (name == null) {
        return new MailAddress(address);
      }
      else {
        try {
          MimeEncodingMethod discard;
          Encoding displayNameEncoding;

          return new MailAddress(address,
                                 MimeEncoding.Decode(name, out discard, out displayNameEncoding),
                                 displayNameEncoding);
        }
        catch (FormatException) {
          return new MailAddress(address, name);
        }
      }
    }

    public static MailAddressCollection ToMailAddressCollection(ImapAddress[] addresses)
    {
      if (addresses == null)
        return null;

      var addressCollection = new MailAddressCollection();

      foreach (var address in addresses) {
        addressCollection.Add(address.ToMailAddress());
      }

      return addressCollection;
    }

    public override string ToString()
    {
      return string.Format("{{Name={0}, AtDomainList={1}, Mailbox={2}, Host={3}}}", name, atDomainList, mailbox, host);
    }

    private /*readonly*/ string name;
    private /*readonly*/ string atDomainList;
    private /*readonly*/ string mailbox;
    private /*readonly*/ string host;
  }
}

/*
address         = "(" addr-name SP addr-adl SP addr-mailbox SP
                  addr-host ")"
addr-adl        = nstring
                    ; Holds route from [RFC-2822] route-addr if
                    ; non-NIL
addr-host       = nstring
                    ; NIL indicates [RFC-2822] group syntax.
                    ; Otherwise, holds [RFC-2822] domain name
addr-mailbox    = nstring
                    ; NIL indicates end of [RFC-2822] group; if
                    ; non-NIL and addr-host is NIL, holds
                    ; [RFC-2822] group name.
                    ; Otherwise, holds [RFC-2822] local-part
                    ; after removing [RFC-2822] quoting
addr-name       = nstring
                    ; If non-NIL, holds phrase from [RFC-2822]
                    ; mailbox after removing [RFC-2822] quoting
 */