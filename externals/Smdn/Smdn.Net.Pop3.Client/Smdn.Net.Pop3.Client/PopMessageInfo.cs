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
using System.Collections.Generic;
using System.IO;
using System.Text;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif
using Smdn.IO;

using Smdn.Net.Pop3.Protocol.Client;

namespace Smdn.Net.Pop3.Client {
  public sealed class PopMessageInfo {
    public PopClient Client {
      get { ThrowIfSessionClosed(); return client; }
      internal set { client = value; }
    }

    public long MessageNumber {
      get { ThrowIfSessionClosed(); return scanList.MessageNumber; }
    }

    public long Length {
      get { ThrowIfSessionClosed(); return scanList.SizeInOctets; }
    }

    public string UniqueId {
      get
      {
        EnsureUniqueIdListed();

        if (uniqueIdList.HasValue)
          return uniqueIdList.Value.UniqueId;
        else
          return null;
      }
    }

    internal PopScanListing ScanList {
      get { return scanList; }
      set { scanList = value; }
    }

    internal PopUniqueIdListing? UniqueIdList {
      get { return uniqueIdList; }
      set
      {
        uniqueIdList = value;
        isUniqueIdListed = true;
      }
    }

    public bool IsMarkedAsDeleted {
      get { ThrowIfSessionClosed(); return isMarkedAsDeleted; }
      internal set { isMarkedAsDeleted = value; }
    }

    internal PopMessageInfo(PopClient client, PopScanListing scanList, PopUniqueIdListing? uniqueIdList)
    {
      this.client = client;
      this.scanList = scanList;
      this.uniqueIdList = uniqueIdList;
      this.isMarkedAsDeleted = false;
      this.isUniqueIdListed = uniqueIdList.HasValue;
    }

    private PopClient client;
    private PopScanListing scanList;
    private PopUniqueIdListing? uniqueIdList;
    private bool isMarkedAsDeleted;
    private bool isUniqueIdListed;

    /*
     * operation
     */
    public void MarkAsDeleted()
    {
      ThrowIfSessionClosed();

      if (isMarkedAsDeleted)
        return;

      client.Delete(this);

      isMarkedAsDeleted = true;
    }

    private void EnsureUniqueIdListed()
    {
      ThrowIfSessionClosed();

      if (isUniqueIdListed)
        return;

      ThrowIfMakredAsDeleted();

      PopUniqueIdListing uid;

      var ret = client.Session.Uidl(MessageNumber, out uid);

      switch (ret.Code) {
        case PopCommandResultCode.Ok:
        case PopCommandResultCode.Error:
          uniqueIdList = uid;
          isUniqueIdListed = true;
          break;

        default:
          PopClient.ThrowIfError(ret);
          break;
      }
    }

    public string GetUniqueId()
    {
      string uniqueId;

      if (TryGetUniqueId(out uniqueId))
        return uniqueId;
      else
        throw new PopIncapableException(PopCapability.Uidl);
    }

    public bool TryGetUniqueId(out string uniqueId)
    {
      EnsureUniqueIdListed();

      if (uniqueIdList.HasValue && uniqueIdList.Value.UniqueId != null) {
        uniqueId = uniqueIdList.Value.UniqueId;
        return true;
      }
      else {
        uniqueId = null;
        return false;
      }
    }

    private const int entireMessage = -1;

    public Stream OpenRead()
    {
      return OpenRead(entireMessage);
    }

    /// <param name="maxLines">
    /// A maximum line count to retrieve.
    /// <list type="number">
    ///   <listheader><term>value</term><description>behavior</description></listheader>
    ///   <item><term>-1</term><description>entire message</description></item>
    ///   <item><term>0</term><description>message without body</description></item>
    ///   <item><term>1 or greater</term><description>message with specified lines of body</description></item>
    /// </list>
    /// </param>
    public Stream OpenRead(int maxLines)
    {
      ThrowIfSessionClosed();
      ThrowIfMakredAsDeleted();

      if (maxLines < -1)
        throw new ArgumentOutOfRangeException("maxLines", maxLines, "must be -1, 0 or positive number");

      Stream messageStream;

      if (maxLines == entireMessage)
        PopClient.ThrowIfError(client.Session.Retr(MessageNumber, out messageStream));
      else
        PopClient.ThrowIfError(client.Session.Top(MessageNumber, maxLines, out messageStream));

      if (client.DeleteAfterRetrieve)
        MarkAsDeleted();

      return messageStream;
    }

    /*
     * ReadAs<TOutput>(Converter<Stream>)
     */
    public TOutput ReadAs<TOutput>(Converter<Stream, TOutput> converter)
    {
      return ReadAs(entireMessage, converter);
    }

    public TOutput ReadAs<TOutput>(int maxLines, Converter<Stream, TOutput> converter)
    {
      if (converter == null)
        throw new ArgumentNullException("converter");

      using (var stream = OpenRead(maxLines)) {
        return converter(stream);
      }
    }

    /*
     * ReadAs<TResult>(Func<Stream, ...>)
     */
    public TResult ReadAs<T, TResult>
      (Func<Stream, T, TResult> read, T arg)
    {
      return ReadAs(entireMessage, read, arg);
    }

    public TResult ReadAs<T1, T2, TResult>
      (Func<Stream, T1, T2, TResult> read, T1 arg1, T2 arg2)
    {
      return ReadAs(entireMessage, read, arg1, arg2);
    }

    public TResult ReadAs<T1, T2, T3, TResult>
      (Func<Stream, T1, T2, T3, TResult> read, T1 arg1, T2 arg2, T3 arg3)
    {
      return ReadAs(entireMessage, read, arg1, arg2, arg3);
    }

    public TResult ReadAs<T1, T2, T3, T4, TResult>
      (Func<Stream, T1, T2, T3, T4, TResult> read, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
      return ReadAs(entireMessage, read, arg1, arg2, arg3, arg4);
    }

    public TResult ReadAs<T, TResult>
      (int maxLines,
       Func<Stream, T, TResult> read, T arg)
    {
      if (read == null)
        throw new ArgumentNullException("read");

      using (var stream = OpenRead(maxLines)) {
        return read(stream, arg);
      }
    }

    public TResult ReadAs<T1, T2, TResult>
      (int maxLines,
       Func<Stream, T1, T2, TResult> read, T1 arg1, T2 arg2)
    {
      if (read == null)
        throw new ArgumentNullException("read");

      using (var stream = OpenRead(maxLines)) {
        return read(stream, arg1, arg2);
      }
    }

    public TResult ReadAs<T1, T2, T3, TResult>
      (int maxLines,
       Func<Stream, T1, T2, T3, TResult> read, T1 arg1, T2 arg2, T3 arg3)
    {
      if (read == null)
        throw new ArgumentNullException("read");

      using (var stream = OpenRead(maxLines)) {
        return read(stream, arg1, arg2, arg3);
      }
    }

    public TResult ReadAs<T1, T2, T3, T4, TResult>
      (int maxLines,
       Func<Stream, T1, T2, T3, T4, TResult> read, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
      if (read == null)
        throw new ArgumentNullException("read");

      using (var stream = OpenRead(maxLines)) {
        return read(stream, arg1, arg2, arg3, arg4);
      }
    }

    /*
     * ReadAllBytes()
     */
    public byte[] ReadAllBytes()
    {
      return ReadAs<byte[]>(ReadAllBytesProc);
    }

    public byte[] ReadAllBytes(int maxLines)
    {
      return ReadAs<byte[]>(maxLines, ReadAllBytesProc);
    }

    private static byte[] ReadAllBytesProc(Stream stream)
    {
      return stream.ReadToEnd();
    }

    /*
     * WriteTo(Stream)
     */
    public void WriteTo(Stream stream)
    {
      WriteTo(stream, entireMessage);
    }

    public void WriteTo(Stream stream, int maxLines)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      using (var messageStream = OpenRead(maxLines)) {
        var chunked = messageStream as ChunkedMemoryStream;

        if (chunked == null)
          messageStream.CopyTo(stream);
        else
          messageStream.CopyTo(stream, chunked.ChunkSize);
      }
    }

    /*
     * WriteTo(BinaryWriter)
     */
    public void WriteTo(BinaryWriter writer)
    {
      WriteTo(writer, entireMessage);
    }

    public void WriteTo(BinaryWriter writer, int maxLines)
    {
      if (writer == null)
        throw new ArgumentNullException("writer");

      using (var messageStream = OpenRead(maxLines)) {
        var chunked = messageStream as ChunkedMemoryStream;

        if (chunked == null)
          messageStream.CopyTo(writer);
        else
          messageStream.CopyTo(writer, chunked.ChunkSize);
      }
    }

    /*
     * Save()
     */
    public void Save(string path)
    {
      Save(path, entireMessage);
    }

    public void Save(string path, int maxLines)
    {
      using (var stream = OpenRead(maxLines)) {
        using (var fileStream = File.OpenWrite(path)) {
          var chunked = stream as ChunkedMemoryStream;

          if (chunked == null)
            stream.CopyTo(fileStream);
          else
            stream.CopyTo(fileStream, chunked.ChunkSize);
        }
      }
    }

    /*
     * OpenText()
     */
    private static readonly Encoding iso8859_1 = Encoding.GetEncoding("ISO-8859-1");

    public StreamReader OpenText()
    {
      return OpenText(iso8859_1, entireMessage);
    }

    public StreamReader OpenText(Encoding encoding)
    {
      return OpenText(encoding, entireMessage);
    }

    public StreamReader OpenText(int maxLines)
    {
      return OpenText(iso8859_1, maxLines);
    }

    public StreamReader OpenText(Encoding encoding, int maxLines)
    {
      if (encoding == null)
        throw new ArgumentNullException("encoding");

      return new StreamReader(OpenRead(maxLines), encoding, false);
    }

    /*
     * ReadAs<TOutput>(Converter<StreamReader>)
     */
    public TOutput ReadAs<TOutput>(Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(), converter);
    }

    public TOutput ReadAs<TOutput>(Encoding encoding, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(encoding), converter);
    }

    public TOutput ReadAs<TOutput>(int maxLines, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(maxLines), converter);
    }

    public TOutput ReadAs<TOutput>(Encoding encoding, int maxLines, Converter<StreamReader, TOutput> converter)
    {
      return ReadAsCore(OpenText(encoding, maxLines), converter);
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
    public IEnumerable<string> ReadLines()
    {
      return ReadLines(iso8859_1, entireMessage);
    }

    public IEnumerable<string> ReadLines(Encoding encoding)
    {
      return ReadLines(encoding, entireMessage);
    }

    public IEnumerable<string> ReadLines(int maxLines)
    {
      return ReadLines(iso8859_1, maxLines);
    }

    public IEnumerable<string> ReadLines(Encoding encoding, int maxLines)
    {
      StreamReader reader = null;

      try {
        reader = OpenText(encoding, maxLines);

        for (;;) {
          var line = reader.ReadLine();

          if (line == null)
            break;
          else
            yield return line;
        }
      }
      finally {
        if (reader != null)
          reader.Close();
      }
    }

    /*
     * ReadAllLines()
     */
    public string[] ReadAllLines()
    {
      return ReadLines().ToArray();
    }

    public string[] ReadAllLines(Encoding encoding)
    {
      return ReadLines(encoding).ToArray();
    }

    public string[] ReadAllLines(int maxLines)
    {
      return ReadLines(maxLines).ToArray();
    }

    public string[] ReadAllLines(Encoding encoding, int maxLines)
    {
      return ReadLines(encoding, maxLines).ToArray();
    }

    /*
     * ReadAllText()
     */
    public string ReadAllText()
    {
      return ReadAs<string>(ReadAllTextProc);
    }

    public string ReadAllText(Encoding encoding)
    {
      return ReadAs<string>(encoding, ReadAllTextProc);
    }

    public string ReadAllText(int maxLines)
    {
      return ReadAs<string>(maxLines, ReadAllTextProc);
    }

    public string ReadAllText(Encoding encoding, int maxLines)
    {
      return ReadAs<string>(encoding, maxLines, ReadAllTextProc);
    }

    private static string ReadAllTextProc(StreamReader reader)
    {
      return reader.ReadToEnd();
    }

    /*
     * utility methods
     */
    internal void ThrowIfMakredAsDeleted()
    {
      if (IsMarkedAsDeleted)
        throw new PopMessageDeletedException(MessageNumber);
    }

    private void ThrowIfSessionClosed()
    {
      if (client == null)
        throw new InvalidOperationException("session has been closed");
    }

    public override string ToString()
    {
      return string.Format("{{PopMessageInfo: Authority='{0}', MessageNumber={1}, Length={2}}}",
                           client.Profile.Authority,
                           MessageNumber,
                           Length);
    }
  }
}
