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
  public sealed class ImapMessageAttribute :
    ImapMessageAttributeBase,
    IImapMessageAttribute
  {
    /*
     * IImapMessageDynamicAttribute
     */
    private ImapMessageDynamicAttribute dynamicAttrs;

    public IImapMessageFlagSet Flags {
      get { return dynamicAttrs.Flags; }
    }

    [CLSCompliant(false)]
    public ulong ModSeq {
      get { return dynamicAttrs.ModSeq; }
    }

    /*
     * IImapMessageStaticAttribute
     */
    private ImapMessageStaticAttribute staticAttrs;

    public Uri Url {
      get { return staticAttrs.Url; }
    }

    public IImapBodyStructure BodyStructure {
      get { return staticAttrs.BodyStructure; }
    }

    public ImapEnvelope Envelope {
      get { return staticAttrs.Envelope; }
    }

    public DateTimeOffset? InternalDate {
      get { return staticAttrs.InternalDate; }
    }

    public long Rfc822Size {
      get { return staticAttrs.Rfc822Size; }
    }

    public long Uid {
      get { return staticAttrs.Uid; }
    }

    public long BinarySize {
      get { return staticAttrs.BinarySize; }
    }

    public long GetBinarySizeOf(string section)
    {
      return staticAttrs.GetBinarySizeOf(section);
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    void IImapUrl.SetBaseUrl(ImapUriBuilder baseUrl)
    {
      (staticAttrs as IImapUrl).SetBaseUrl(baseUrl);
    }

    internal ImapMessageAttribute(long sequence)
      : base(sequence)
    {
      this.dynamicAttrs = new ImapMessageDynamicAttribute(sequence);
      this.staticAttrs = new ImapMessageStaticAttribute(sequence);
    }

    public ImapMessageDynamicAttribute GetDynamicAttributeImpl()
    {
      return dynamicAttrs;
    }

    public ImapMessageStaticAttribute GetStaticAttributeImpl()
    {
      return staticAttrs;
    }

    public override string ToString()
    {
      return string.Format("{{Sequence={0}, Uid={1}, BodyStructure={2}, Envelope={3}, InternalDate={4}, Rfc822Size={5}, Flags={6}, ModSeq={7}}}",
                           Sequence,
                           Uid,
                           BodyStructure,
                           Envelope,
                           InternalDate,
                           Rfc822Size,
                           Flags,
                           ModSeq);
    }
  }
}
