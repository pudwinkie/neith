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

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Formats;
using Smdn.Formats.Mime;
using Smdn.Net.Imap4.Client.Session;

namespace Smdn.Net.Imap4.Client {
  public sealed partial class ImapMessageInfo : ImapMessageInfoBase, IImapMessageAttribute {
    public static readonly long ExpungedMessageSequenceNumber = 0L;

    public long Sequence {
      get; internal set;
    }

    public long Uid {
      get; private set;
    }

    public Uri Url {
      get { return uriBuilder.Uri; }
    }

    /*
     * IImapMessageDynamicAttribute
     */
    private ImapMessageDynamicAttribute dynamicAttribute;

    internal ImapMessageDynamicAttribute DynamicAttribute {
      get { EnsureDynamicAttributesFetched(false); return dynamicAttribute; }
      set { dynamicAttribute = value; }
    }

    internal ImapMessageDynamicAttribute GetDynamicAttribute()
    {
      return dynamicAttribute;
    }

    public IImapMessageFlagSet Flags {
      get { return DynamicAttribute.Flags; }
    }

    [CLSCompliant(false)]
    public ulong ModSeq {
      get { return DynamicAttribute.ModSeq; }
    }

    /*
     * IImapMessageStaticAttribute
     */
    private ImapMessageStaticAttribute staticAttribute;

    internal ImapMessageStaticAttribute StaticAttribute {
      get { EnsureStaticAttributesFetched(); return staticAttribute; }
      set { staticAttribute = value; }
    }

    public IImapBodyStructure BodyStructure {
      get { return StaticAttribute.BodyStructure; }
    }

    public ImapEnvelope Envelope {
      get { return StaticAttribute.Envelope; }
    }

    public DateTimeOffset InternalDate {
      get { return StaticAttribute.InternalDate.Value; }
    }

    DateTimeOffset? IImapMessageStaticAttribute.InternalDate {
      get { return StaticAttribute.InternalDate; }
    }

    public long Length {
      get { return StaticAttribute.Rfc822Size; }
    }

    long IImapMessageStaticAttribute.Rfc822Size {
      get { return StaticAttribute.Rfc822Size; }
    }

    long IImapMessageStaticAttribute.GetBinarySizeOf(string section)
    {
      return StaticAttribute.GetBinarySizeOf(section);
    }

    /*
     * miscellaneous getter properties
     */
    public bool IsMarkedAsDeleted {
      get { return Flags.Contains(ImapMessageFlag.Deleted); }
    }

    public bool IsSeen {
      get { return Flags.Contains(ImapMessageFlag.Seen); }
    }

    public bool IsRecent {
      get { return Flags.Contains(ImapMessageFlag.Recent); }
    }

    public bool IsAnswered {
      get { return Flags.Contains(ImapMessageFlag.Answered); }
    }

    public bool IsDraft {
      get { return Flags.Contains(ImapMessageFlag.Draft); }
    }

    public bool IsFlagged {
      get { return Flags.Contains(ImapMessageFlag.Flagged); }
    }

    public bool IsDeleted {
      get { return Sequence == ExpungedMessageSequenceNumber; }
    }

    private Lazy<string> envelopeSubject;

    public string EnvelopeSubject {
      get { return envelopeSubject.Value; }
    }

    private Lazy<DateTimeOffset?> envelopeDate;

    public DateTimeOffset? EnvelopeDate {
      get { return envelopeDate.Value; }
    }

    public MimeType MediaType {
      get { return StaticAttribute.BodyStructure.MediaType; }
    }

    public bool IsMultiPart {
      get { return StaticAttribute.BodyStructure.IsMultiPart; }
    }

    private ImapSequenceSet uidSet;

    private ImapSequenceSet SequenceOrUidSet {
      get
      {
        if (uidSet == null)
          uidSet = ImapSequenceSet.CreateUidSet(Uid);

        return uidSet;
      }
    }

    internal ImapMessageInfo(ImapOpenedMailboxInfo mailbox, long uid, long sequence)
      : base(mailbox)
    {
      this.Uid = uid;
      this.Sequence = sequence;

      (this as IImapUrl).SetBaseUrl(mailbox.Mailbox.UrlBuilder);

      InitializeLazy();
    }

    private ImapUriBuilder uriBuilder;

    private void InitializeLazy()
    {
      envelopeSubject = new Lazy<string>(delegate {
        try {
          return MimeEncoding.DecodeNullable(StaticAttribute.Envelope.Subject);
        }
        catch (FormatException) {
          return StaticAttribute.Envelope.Subject;
        }
      });

      envelopeDate = new Lazy<DateTimeOffset?>(delegate {
        try {
          return DateTimeConvert.FromRFC822DateTimeOffsetStringNullable(StaticAttribute.Envelope.Date);
        }
        catch (FormatException) {
          return default(DateTimeOffset);
        }
      });
    }

    protected override ImapSequenceSet GetSequenceOrUidSet()
    {
      return SequenceOrUidSet;
    }

    protected override void PrepareOperation()
    {
      if (IsDeleted)
        throw new ImapMessageDeletedException(this);
    }

    /// <summary>Infrastructure. It is not intended to be used directly from your code.</summary>
    void IImapUrl.SetBaseUrl(ImapUriBuilder baseUrl)
    {
      uriBuilder = baseUrl.Clone();

      uriBuilder.Uid = Uid;
      uriBuilder.UidValidity = UidValidity;
    }

    public override string ToString()
    {
      return string.Format("{{ImapMessageInfo: Authority='{0}', Mailbox='{1}', Sequence={2}, Uid={3}, UidValidity={4}}}",
                           ImapStyleUriParser.GetStrongAuthority(Url),
                           Mailbox.FullName,
                           Sequence,
                           Uid,
                           UidValidity);
    }
  }
}
