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
using System.Runtime.Serialization;

using Smdn.IO;

namespace Smdn.Net.Imap4.Protocol {
  // handles '4. Data Formats'
  [Serializable]
  public sealed class ImapData : ISerializable {
    public ImapDataFormat Format {
      get { return format; }
    }

    // handles '4.4. Parenthesized List'
    public ImapData[] List {
      get { return list; }
    }

    private ImapData(ImapDataFormat format)
    {
      this.format = format;
    }

    private ImapData(SerializationInfo info, StreamingContext context)
    {
      this.format = (ImapDataFormat)info.GetValue("format", typeof(ImapDataFormat));

      switch (this.format) {
        case ImapDataFormat.List:
          this.list = (ImapData[])info.GetValue("list", typeof(ImapData[]));
          break;

        case ImapDataFormat.Text:
          var t = (byte[])info.GetValue("text", typeof(byte[]));
          this.text = ByteString.CreateImmutable(t); // TODO: chunked
          break;

        case ImapDataFormat.Nil:
        case ImapDataFormat.None:
        default:
          break;
      }
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("format", format);

      switch (format) {
        case ImapDataFormat.List:
          info.AddValue("list", list);
          break;

        case ImapDataFormat.Text:
          info.AddValue("text", GetTextAsByteArray());
          break;

        case ImapDataFormat.Nil:
        case ImapDataFormat.None:
        default:
          break;
      }
    }

    public static ImapData CreateTextData(ByteString text)
    {
      if (text == null)
        throw new ArgumentNullException("text");

      // 4.1. Atom, 4.2. Number, 4.3. String
      var data = new ImapData(ImapDataFormat.Text);

      data.text = text;

      return data;
    }

    public static ImapData CreateTextData(Stream textStream)
    {
      if (textStream == null)
        throw new ArgumentNullException("textStream");

      // 4.1. Atom, 4.2. Number, 4.3. String
      var data = new ImapData(ImapDataFormat.Text);

      data.textStream = textStream;

      return data;
    }

    public static ImapData CreateListData(ImapData[] list)
    {
      if (list == null)
        throw new ArgumentNullException("list");

      // 4.4. Parenthesized List
      var data = new ImapData(ImapDataFormat.List);

      data.list = list;

      return data;
    }

    private static ImapData nil = new ImapData(ImapDataFormat.Nil);

    public static ImapData CreateNilData()
    {
      // 4.5. NIL
      return nil;
    }

#region "text conversion methods"
    // handles '4.1. Atom'
    //         '4.2. Number'
    //         '4.3. String'

    public string GetTextAsString()
    {
      CheckIsText();

      if (textStream == null)
        return text.ToString();
      else
        return ByteString.CreateImmutable(InternalGetTextAsByteArray()).ToString();
    }

    public ByteString GetTextAsByteString()
    {
      CheckIsText();

      if (textStream == null)
        return text;
      else
        return ByteString.CreateImmutable(InternalGetTextAsByteArray());
    }

    [CLSCompliant(false)]
    public ulong GetTextAsNumber()
    {
      CheckIsText();

      if (textStream == null) {
        try {
          return text.ToUInt64();
        }
        catch (FormatException) {
          throw new ImapMalformedDataException(this);
        }
      }
      else {
        throw new NotImplementedException();
      }
    }

    public byte[] GetTextAsByteArray()
    {
      CheckIsText();

      if (textStream == null)
        return text.ToArray();
      else
        return InternalGetTextAsByteArray();
    }

    private byte[] InternalGetTextAsByteArray()
    {
      try {
        textStream.Position = 0L;

        var chunkedStream = textStream as ChunkedMemoryStream;

        if (chunkedStream == null)
          return textStream.ReadToEnd();
        else
          return chunkedStream.ToArray();
      }
      finally {
        textStream.Position = 0L;
      }
    }

    public Stream GetTextAsStream()
    {
      CheckIsText();

      if (textStream == null)
        return new MemoryStream(text.Segment.Array,
                                text.Segment.Offset,
                                text.Segment.Count,
                                false);
      else
        return textStream;
    }

    public long GetTextLength()
    {
      CheckIsText();

      if (textStream == null)
        return text.Length;
      else
        return textStream.Length;
    }

    public void CopyText(byte[] buffer, int offset, int count)
    {
      CheckIsText();

      if (textStream == null) {
        Buffer.BlockCopy(text.Segment.Array, text.Segment.Offset,
                         buffer, offset, count);
      }
      else {
        try {
          textStream.Position = 0L;
          textStream.Read(buffer, offset, count);
        }
        finally {
          textStream.Position = 0L;
        }
      }
    }

    public int GetText(ref byte[] buffer)
    {
      CheckIsText();

      if (textStream == null) {
        if (buffer == null || buffer.Length < text.Length)
          buffer = new byte[text.Length];

        Buffer.BlockCopy(text.Segment.Array, text.Segment.Offset,
                         buffer, 0, text.Length);

        return text.Length;
      }
      else {
        if (buffer == null || buffer.Length < textStream.Length)
          buffer = new byte[(int)textStream.Length];

        try {
          textStream.Position = 0L;

          return textStream.Read(buffer, 0, buffer.Length);
        }
        finally {
          textStream.Position = 0L;
        }
      }
    }

    private void CheckIsText()
    {
      if (format != ImapDataFormat.Text)
        throw new InvalidOperationException("data is not text");
    }
#endregion

    public override string ToString()
    {
      switch (format) {
        case ImapDataFormat.Text:
          if (textStream == null)
            return string.Format("{{Text:{0}}}", text);
          else
            return string.Format("{{Text:<Stream ({0} octets)>}}", textStream.Length);
        case ImapDataFormat.List:
          return string.Format("{{List:{0}}}", string.Join(", ", Array.ConvertAll(list, delegate(ImapData d) {
            return d.ToString();
          })));
        case ImapDataFormat.Nil:
          return "{NIL}";
        default:
          return "{unknown}";
      }
    }

    private readonly ImapDataFormat format;
    private ByteString text = null;
    private Stream textStream = null;
    private ImapData[] list = null;
  }
}