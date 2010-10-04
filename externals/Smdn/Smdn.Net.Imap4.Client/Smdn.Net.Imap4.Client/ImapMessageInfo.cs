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
using System.IO;
using System.Security.Cryptography;
using System.Text;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.Formats;
using Smdn.Formats.Mime;
using Smdn.IO;
using Smdn.Net.Imap4.Client.Session;
using Smdn.Net.Imap4.Protocol.Client;

namespace Smdn.Net.Imap4.Client {
  public sealed class ImapMessageInfo : ImapMessageInfoBase, IImapMessageAttribute {
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
      get { return Flags.Has(ImapMessageFlag.Deleted); }
    }

    public bool IsSeen {
      get { return Flags.Has(ImapMessageFlag.Seen); }
    }

    public bool IsRecent {
      get { return Flags.Has(ImapMessageFlag.Recent); }
    }

    public bool IsAnswered {
      get { return Flags.Has(ImapMessageFlag.Answered); }
    }

    public bool IsDraft {
      get { return Flags.Has(ImapMessageFlag.Draft); }
    }

    public bool IsFlagged {
      get { return Flags.Has(ImapMessageFlag.Flagged); }
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

    /*
     * operations
     */
    public void Refresh()
    {
      EnsureDynamicAttributesFetched(true);
    }

    private bool FindBySequence(ImapMessageAttributeBase attribute)
    {
      return attribute.Sequence == Sequence;
    }

    internal static readonly ImapFetchDataItem FetchStaticDataItem =
      ImapFetchDataItem.BodyStructure +
      ImapFetchDataItem.Envelope + 
      ImapFetchDataItem.InternalDate +
      ImapFetchDataItem.Rfc822Size;

    private void EnsureStaticAttributesFetched()
    {
      if (staticAttribute != null)
        return;

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, SequenceOrUidSet);

      PrepareOperation();

      ImapMessageStaticAttribute[] staticAttrs;

      Mailbox.ProcessResult(Client.Session.Fetch(SequenceOrUidSet,
                                                 FetchStaticDataItem,
                                                 out staticAttrs));

      var staticAttr = staticAttrs.FirstOrDefault(FindBySequence);

      if (staticAttr == null) {
        Sequence = ExpungedMessageSequenceNumber;
        throw new ImapMessageDeletedException(this);
      }
      else {
        staticAttribute = staticAttr;
      }
    }

    internal static ImapFetchDataItem GetFetchDynamicDataItem(ImapOpenedMailboxInfo mailbox)
    {
      if (mailbox.IsModSequencesAvailable)
        return ImapFetchDataItem.Flags + ImapFetchDataItem.ModSeq;
      else
        return ImapFetchDataItem.Flags;
    }

    internal static ImapFetchDataItem TranslateFetchOption(ImapOpenedMailboxInfo mailbox,
                                                           ImapMessageFetchAttributeOptions options,
                                                           out bool fetchStaticAttr,
                                                           out bool fetchDynamicAttr)
    {
      fetchStaticAttr = (int)(options & ImapMessageFetchAttributeOptions.StaticAttributes) != 0;
      fetchDynamicAttr = (int)(options & ImapMessageFetchAttributeOptions.DynamicAttributes) != 0;

      var fetchDataItem = ImapFetchDataItem.Uid;

      if (fetchStaticAttr)
        fetchDataItem += ImapMessageInfo.FetchStaticDataItem;
      if (fetchDynamicAttr)
        fetchDataItem += ImapMessageInfo.GetFetchDynamicDataItem(mailbox);

      return fetchDataItem;
    }

    private void EnsureDynamicAttributesFetched(bool refresh)
    {
      if (!refresh && dynamicAttribute != null)
        return;

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, SequenceOrUidSet);

      PrepareOperation();

      ImapMessageDynamicAttribute[] dynamicAttrs;

      Mailbox.ProcessResult(Client.Session.Fetch(SequenceOrUidSet,
                                                 GetFetchDynamicDataItem(Mailbox),
                                                 out dynamicAttrs));

      var dynamicAttr = dynamicAttrs.FirstOrDefault(FindBySequence);

      if (dynamicAttr == null) {
        Sequence = ExpungedMessageSequenceNumber;
        throw new ImapMessageDeletedException(this);
      }
      else {
        dynamicAttribute = dynamicAttr;
      }
    }

    public void ToggleFlags(ImapMessageFlag flag, params ImapMessageFlag[] flags)
    {
      ToggleFlags(new ImapMessageFlagList(flags.Prepend(flag)));
    }

    public void ToggleKeywords(string keyword, params string[] keywords)
    {
      ToggleFlags(new ImapMessageFlagList(keywords.Prepend(keyword)));
    }

    public void ToggleFlags(IImapMessageFlagSet flagsAndKeywords)
    {
      if (flagsAndKeywords == null)
        throw new ArgumentNullException("flagsAndKeywords");

      var addFlags = new ImapMessageFlagList();
      var removeFlags = new ImapMessageFlagList();

      EnsureDynamicAttributesFetched(false);

      foreach (var flag in flagsAndKeywords) {
        if (dynamicAttribute.Flags.Has(flag))
          removeFlags.Add(flag);
        else
          addFlags.Add(flag);
      }

      if (0 < removeFlags.Count)
        Store(ImapStoreDataItem.RemoveFlags(removeFlags));

      if (0 < addFlags.Count)
        Store(ImapStoreDataItem.AddFlags(addFlags));
    }

    public IImapBodyStructure GetStructureOf(params int[] subSections)
    {
      if (subSections == null || subSections.Length == 0)
        return BodyStructure;
      else
        return ImapBodyStructureUtils.FindSection(BodyStructure, subSections);
    }

    public TBodyStructure GetStructureOf<TBodyStructure>(params int[] subSections)
      where TBodyStructure : IImapBodyStructure
    {
      return (TBodyStructure)GetStructureOf(subSections);
    }

    public IImapBodyStructure GetStructureOf(string section)
    {
      if (string.IsNullOrEmpty(section))
        return BodyStructure;
      else
        return ImapBodyStructureUtils.FindSection(BodyStructure, section);
    }

    public TBodyStructure GetStructureOf<TBodyStructure>(string section)
      where TBodyStructure : IImapBodyStructure
    {
      return (TBodyStructure)GetStructureOf(section);
    }

    /*
     * utility methods for Open*(), Read*()
     */
    private string GetSection(IImapBodyStructure structure)
    {
      if (staticAttribute == null /* not fetched yed */ ||
          !object.ReferenceEquals(staticAttribute.BodyStructure, structure.GetRootStructure()))
        throw new ArgumentException("invalid structure", "structure");

      return structure.Section;
    }

    private static readonly Encoding iso8859_1 = Encoding.GetEncoding("ISO-8859-1");

    private static string GetEncoding(IImapBodyStructure structure, ImapMessageFetchBodyOptions options)
    {
      Encoding discard;

      return GetEncoding(structure, options, out discard);
    }

    private static string GetEncoding(IImapBodyStructure structure, ImapMessageFetchBodyOptions options, out Encoding charset)
    {
      var s = structure as ImapSinglePartBodyStructure;

      if (s != null && (int)(options & ImapMessageFetchBodyOptions.DecodeContent) != 0) {
        string charsetString;

        if (s.Parameters.TryGetValue("charset", out charsetString))
          charset = EncodingUtils.GetEncodingThrowException(charsetString);
        else
          charset = iso8859_1;

        return s.Encoding;
      }
      else {
        charset = null;

        return null;
      }
    }

    /*
     * OpenRead()
     */
    public Stream OpenRead(IImapBodyStructure section)
    {
      return OpenRead(GetSection(section), null, null, ImapMessageFetchBodyOptions.Default);
    }

    public Stream OpenRead(IImapBodyStructure section, ImapMessageFetchBodyOptions options)
    {
      return OpenRead(GetSection(section), null, GetEncoding(section, options), options);
    }

    public Stream OpenRead()
    {
      return OpenRead(string.Empty, null, null, ImapMessageFetchBodyOptions.Default);
    }

    public Stream OpenRead(ImapMessageFetchBodyOptions options)
    {
      return OpenRead(string.Empty, null, null, options);
    }

    public Stream OpenRead(string section)
    {
      return OpenRead(section, null, null, ImapMessageFetchBodyOptions.Default);
    }

    public Stream OpenRead(string section, ImapPartialRange? range)
    {
      return OpenRead(section, range, null, ImapMessageFetchBodyOptions.Default);
    }

    public Stream OpenRead(string section, ImapMessageFetchBodyOptions options)
    {
      return OpenRead(section, null, null, options);
    }

    public Stream OpenRead(string section, ImapPartialRange? range, ImapMessageFetchBodyOptions options)
    {
      return OpenRead(section, range, null, options);
    }

    private Stream OpenRead(string section, ImapPartialRange? range, string encoding, ImapMessageFetchBodyOptions options)
    {
      var setSeen = (int)(options & ImapMessageFetchBodyOptions.SetSeen) != 0;

      Mailbox.CheckSelected();
      Mailbox.CheckUidValidity(UidValidity, SequenceOrUidSet);

      PrepareOperation();

      var stream = new FetchMessageBodyStream(this, !setSeen, section, range);

      IImapMessageAttribute discard;

      Mailbox.ProcessResult(stream.Prepare(null, out discard));

      if (encoding != null)
        return ContentTransferEncoding.CreateDecodingStream(stream, encoding);
      else
        return stream;
    }

    private class FetchMessageBodyStream : ImapFetchMessageBodyStream {
      public ImapMessageInfo Message {
        get; private set;
      }

      public FetchMessageBodyStream(ImapMessageInfo message,
                                    bool peek,
                                    string fetchSection,
                                    ImapPartialRange? fetchRange)
        : base(message.Client.Session, peek, message.SequenceOrUidSet, fetchSection, fetchRange, DefaultFetchBlockSize)
      {
        this.Message = message;
      }

      protected override Exception GetNoSuchMessageException(ImapCommandResult result)
      {
        Message.Mailbox.ProcessResult(result);

        Message.Sequence = ExpungedMessageSequenceNumber;

        return new ImapMessageDeletedException(Message);
      }

      protected override Exception GetFetchFailureException(ImapCommandResult result)
      {
        Message.Mailbox.ProcessResult(result);

        return base.GetFetchFailureException(result);
      }
    }

    /*
     * ReadAs<Stream>()
     */
    public TOutput ReadAs<TOutput>(IImapBodyStructure section, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(section), converter);
    }

    public TOutput ReadAs<TOutput>(IImapBodyStructure section, ImapMessageFetchBodyOptions options, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(section, options), converter);
    }

    public TOutput ReadAs<TOutput>(Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(), converter);
    }

    public TOutput ReadAs<TOutput>(ImapMessageFetchBodyOptions options, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(options), converter);
    }

    public TOutput ReadAs<TOutput>(string section, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(section), converter);
    }

    public TOutput ReadAs<TOutput>(string section, ImapMessageFetchBodyOptions options, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(section, options), converter);
    }

    public TOutput ReadAs<TOutput>(string section, ImapPartialRange? range, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(section, range), converter);
    }

    public TOutput ReadAs<TOutput>(string section, ImapPartialRange? range, ImapMessageFetchBodyOptions options, Converter<Stream, TOutput> converter)
    {
      return ReadAsCore(OpenRead(section, range, options), converter);
    }

    private TOutput ReadAsCore<TOutput>(Stream stream, Converter<Stream, TOutput> converter)
    {
      using (stream) {
        if (converter == null)
          throw new ArgumentNullException("converter");

        return converter(stream);
      }
    }

    /*
     * ReadAllBytes()
     */
    public byte[] ReadAllBytes(IImapBodyStructure section)
    {
      return ReadAs<byte[]>(section, ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(IImapBodyStructure section, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<byte[]>(section, options, ReadAllBytesProc);
    }

    public byte[] ReadAllBytes()
    {
      return ReadAs<byte[]>(ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(ImapMessageFetchBodyOptions options)
    {
      return ReadAs<byte[]>(options, ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(string section)
    {
      return ReadAs<byte[]>(section, ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(string section, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<byte[]>(section, options, ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(string section, ImapPartialRange? range)
    {
      return ReadAs<byte[]>(section, range, ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(string section, ImapPartialRange? range, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<byte[]>(section, range, options, ReadAllBytesProc);
    }

    private static byte[] ReadAllBytesProc(Stream stream)
    {
      return stream.ReadToEnd();
    }

    /*
     * WriteTo(Stream)
     */
    public void WriteTo(Stream stream, IImapBodyStructure section)
    {
      WriteToCore(stream, OpenRead(section));
    }

    public void WriteTo(Stream stream, IImapBodyStructure section, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(stream, OpenRead(section, options));
    }

    public void WriteTo(Stream stream)
    {
      WriteToCore(stream, OpenRead());
    }

    public void WriteTo(Stream stream, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(stream, OpenRead(options));
    }

    public void WriteTo(Stream stream, string section)
    {
      WriteToCore(stream, OpenRead(section));
    }

    public void WriteTo(Stream stream, string section, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(stream, OpenRead(section, options));
    }

    public void WriteTo(Stream stream, string section, ImapPartialRange? range)
    {
      WriteToCore(stream, OpenRead(section, range));
    }

    public void WriteTo(Stream stream, string section, ImapPartialRange? range, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(stream, OpenRead(section, range, options));
    }

    private void WriteToCore(Stream writeToStream, Stream stream)
    {
      using (stream) {
        if (writeToStream == null)
          throw new ArgumentNullException("writeToStream");

        stream.CopyTo(writeToStream, ImapFetchMessageBodyStream.DefaultFetchBlockSize);
      }
    }

    /*
     * WriteTo(BinaryWriter)
     */
    public void WriteTo(BinaryWriter writer, IImapBodyStructure section)
    {
      WriteToCore(writer, OpenRead(section));
    }

    public void WriteTo(BinaryWriter writer, IImapBodyStructure section, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(writer, OpenRead(section, options));
    }

    public void WriteTo(BinaryWriter writer)
    {
      WriteToCore(writer, OpenRead());
    }

    public void WriteTo(BinaryWriter writer, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(writer, OpenRead(options));
    }

    public void WriteTo(BinaryWriter writer, string section)
    {
      WriteToCore(writer, OpenRead(section));
    }

    public void WriteTo(BinaryWriter writer, string section, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(writer, OpenRead(section, options));
    }

    public void WriteTo(BinaryWriter writer, string section, ImapPartialRange? range)
    {
      WriteToCore(writer, OpenRead(section, range));
    }

    public void WriteTo(BinaryWriter writer, string section, ImapPartialRange? range, ImapMessageFetchBodyOptions options)
    {
      WriteToCore(writer, OpenRead(section, range, options));
    }

    private void WriteToCore(BinaryWriter writer, Stream stream)
    {
      using (stream) {
        if (writer == null)
          throw new ArgumentNullException("writer");

        stream.CopyTo(writer, ImapFetchMessageBodyStream.DefaultFetchBlockSize);
      }
    }

    /*
     * Save()
     */
    public void Save(string path, IImapBodyStructure section)
    {
      SaveCore(path, delegate { return OpenRead(section); });
    }

    public void Save(string path, IImapBodyStructure section, ImapMessageFetchBodyOptions options)
    {
      SaveCore(path, delegate { return OpenRead(section, options); });
    }

    public void Save(string path)
    {
      SaveCore(path, delegate { return OpenRead(); });
    }

    public void Save(string path, ImapMessageFetchBodyOptions options)
    {
      SaveCore(path, delegate { return OpenRead(options); });
    }

    public void Save(string path, string section)
    {
      SaveCore(path, delegate { return OpenRead(section); });
    }

    public void Save(string path, string section, ImapMessageFetchBodyOptions options)
    {
      SaveCore(path, delegate { return OpenRead(section, options); });
    }

    public void Save(string path, string section, ImapPartialRange? range)
    {
      SaveCore(path, delegate { return OpenRead(section, range); });
    }

    public void Save(string path, string section, ImapPartialRange? range, ImapMessageFetchBodyOptions options)
    {
      SaveCore(path, delegate { return OpenRead(section, range, options); });
    }

    private void SaveCore(string path, Func<Stream> openRead)
    {
      using (var stream = openRead()) {
        using (var fileStream = File.OpenWrite(path)) {
          stream.CopyTo(fileStream, ImapFetchMessageBodyStream.DefaultFetchBlockSize);
        }
      }
    }

    /*
     * OpenText
     */
    public StreamReader OpenText(IImapBodyStructure section)
    {
      return OpenText(section, ImapMessageFetchBodyOptions.Default);
    }

    public StreamReader OpenText(IImapBodyStructure section, ImapMessageFetchBodyOptions options)
    {
      Encoding charset;
      var stream = OpenRead(GetSection(section), null, GetEncoding(section, options, out charset), options);

      return new StreamReader(stream, charset ?? iso8859_1);
    }

    public StreamReader OpenText()
    {
      return OpenText(string.Empty, iso8859_1, ImapMessageFetchBodyOptions.Default);
    }

    public StreamReader OpenText(ImapMessageFetchBodyOptions options)
    {
      return OpenText(string.Empty, iso8859_1, options);
    }

    public StreamReader OpenText(string section)
    {
      return OpenText(section, iso8859_1, ImapMessageFetchBodyOptions.Default);
    }

    public StreamReader OpenText(string section, ImapMessageFetchBodyOptions options)
    {
      return OpenText(section, iso8859_1, options);
    }

    public StreamReader OpenText(Encoding charset)
    {
      return OpenText(string.Empty, charset, ImapMessageFetchBodyOptions.Default);
    }

    public StreamReader OpenText(Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return OpenText(string.Empty, charset, options);
    }

    public StreamReader OpenText(string section, Encoding charset)
    {
      return OpenText(section, charset, ImapMessageFetchBodyOptions.Default);
    }

    public StreamReader OpenText(string section, Encoding charset, ImapMessageFetchBodyOptions options)
    {
      if (charset == null)
        throw new ArgumentNullException("charset");

      return new StreamReader(OpenRead(section, null, null, options), charset);
    }

    /*
     * ReadAs<StreamReader>()
     */
    public TOutput ReadAs<TOutput>(Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(), converter);
    }

    public TOutput ReadAs<TOutput>(ImapMessageFetchBodyOptions options, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(options), converter);
    }

    public TOutput ReadAs<TOutput>(IImapBodyStructure section, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(section), converter);
    }

    public TOutput ReadAs<TOutput>(IImapBodyStructure section, ImapMessageFetchBodyOptions options, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(section, options), converter);
    }

    public TOutput ReadAs<TOutput>(string section, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(section), converter);
    }

    public TOutput ReadAs<TOutput>(string section, ImapMessageFetchBodyOptions options, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(section, options), converter);
    }

    public TOutput ReadAs<TOutput>(Encoding charset, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(charset), converter);
    }

    public TOutput ReadAs<TOutput>(Encoding charset, ImapMessageFetchBodyOptions options, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(charset, options), converter);
    }

    public TOutput ReadAs<TOutput>(string section, Encoding charset, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(section, charset), converter);
    }

    public TOutput ReadAs<TOutput>(string section, Encoding charset, ImapMessageFetchBodyOptions options, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(section, charset, options), converter);
    }

    private TOutput ReadAsCore<TOutput>(StreamReader reader, Converter<StreamReader, TOutput> converter)
    {
      try {
        if (converter == null)
          throw new ArgumentNullException("converter");

        return converter(reader);
      }
      finally {
        reader.Close();
      }
    }

    /*
     * ReadLines()
     */
    public IEnumerable<string> ReadLines(IImapBodyStructure structure)
    {
      return ReadLinesCore(OpenText(structure));
    }

    public IEnumerable<string> ReadLines(IImapBodyStructure structure, ImapMessageFetchBodyOptions options)
    {
      return ReadLinesCore(OpenText(structure, options));
    }

    public IEnumerable<string> ReadLines()
    {
      return ReadLinesCore(OpenText());
    }

    public IEnumerable<string> ReadLines(ImapMessageFetchBodyOptions options)
    {
      return ReadLinesCore(OpenText(options));
    }

    public IEnumerable<string> ReadLines(string section)
    {
      return ReadLinesCore(OpenText(section));
    }

    public IEnumerable<string> ReadLines(string section, ImapMessageFetchBodyOptions options)
    {
      return ReadLinesCore(OpenText(section, options));
    }

    public IEnumerable<string> ReadLines(Encoding charset)
    {
      return ReadLinesCore(OpenText(charset));
    }

    public IEnumerable<string> ReadLines(Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return ReadLinesCore(OpenText(charset, options));
    }

    public IEnumerable<string> ReadLines(string section, Encoding charset)
    {
      return ReadLinesCore(OpenText(section, charset));
    }

    public IEnumerable<string> ReadLines(string section, Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return ReadLinesCore(OpenText(section, charset, options));
    }

    private IEnumerable<string> ReadLinesCore(StreamReader reader)
    {
      try {
        for (;;) {
          var line = reader.ReadLine();

          if (line == null)
            break;
          else
            yield return line;
        }
      }
      finally {
        reader.Close();
      }
    }

    /*
     * ReadAllLines()
     */
    public string[] ReadAllLines(IImapBodyStructure structure)
    {
      return ReadLines(structure).ToArray();
    }

    public string[] ReadAllLines(IImapBodyStructure structure, ImapMessageFetchBodyOptions options)
    {
      return ReadLines(structure, options).ToArray();
    }

    public string[] ReadAllLines()
    {
      return ReadLines().ToArray();
    }

    public string[] ReadAllLines(ImapMessageFetchBodyOptions options)
    {
      return ReadLines(options).ToArray();
    }

    public string[] ReadAllLines(string section)
    {
      return ReadLines(section).ToArray();
    }

    public string[] ReadAllLines(string section, ImapMessageFetchBodyOptions options)
    {
      return ReadLines(section, options).ToArray();
    }

    public string[] ReadAllLines(Encoding charset)
    {
      return ReadLines(charset).ToArray();
    }

    public string[] ReadAllLines(Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return ReadLines(charset, options).ToArray();
    }

    public string[] ReadAllLines(string section, Encoding charset)
    {
      return ReadLines(section, charset).ToArray();
    }

    public string[] ReadAllLines(string section, Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return ReadLines(section, charset, options).ToArray();
    }

    /*
     * ReadAllText()
     */
    public string ReadAllText(IImapBodyStructure structure)
    {
      return ReadAs<string>(structure, ReadAllTextProc);
    }

    public string ReadAllText(IImapBodyStructure structure, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<string>(structure, options, ReadAllTextProc);
    }

    public string ReadAllText()
    {
      return ReadAs<string>(ReadAllTextProc);
    }

    public string ReadAllText(ImapMessageFetchBodyOptions options)
    {
      return ReadAs<string>(options, ReadAllTextProc);
    }

    public string ReadAllText(string section)
    {
      return ReadAs<string>(section, ReadAllTextProc);
    }

    public string ReadAllText(string section, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<string>(section, options, ReadAllTextProc);
    }

    public string ReadAllText(Encoding charset)
    {
      return ReadAs<string>(charset, ReadAllTextProc);
    }

    public string ReadAllText(Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<string>(charset, options, ReadAllTextProc);
    }

    public string ReadAllText(string section, Encoding charset)
    {
      return ReadAs<string>(section, charset, ReadAllTextProc);
    }

    public string ReadAllText(string section, Encoding charset, ImapMessageFetchBodyOptions options)
    {
      return ReadAs<string>(section, charset, options, ReadAllTextProc);
    }

    private static string ReadAllTextProc(StreamReader reader)
    {
      return reader.ReadToEnd();
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
