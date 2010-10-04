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
using System.Text;

using Smdn.IO;

namespace Smdn.Formats.Mime {
  public class MimeMessage {
    public MimeFormat Format {
      get; set;
    }

    public MimeHeaderCollection Headers {
      get { return headers; }
    }

    public MimeType MimeType {
      get; set;
    }

    public Encoding Charset {
      get; set;
    }

    public string Boundary {
      get; set;
    }

    public ContentTransferEncodingMethod TransferEncoding {
      get; set;
    }

    public MimeMessageDisposition Disposition {
      get; set;
    }

    internal MemoryStream Content {
      get; set;
    }

    public List<MimeMessage> SubParts {
      get { return subParts; }
    }

    public MimeMessage()
      : this(new MimeHeaderCollection(), null, new MimeMessage[] {})
    {
      this.MimeType = MimeType.TextPlain;
    }

    public MimeMessage(IEnumerable<MimeHeader> headers)
      : this(new MimeHeaderCollection(headers), null, new MimeMessage[] {})
    {
      this.MimeType = MimeType.TextPlain;
    }

    public MimeMessage(MimeHeaderCollection headers)
      : this(headers, null, new MimeMessage[] {})
    {
      this.MimeType = MimeType.TextPlain;
    }

    public MimeMessage(IEnumerable<MimeMessage> subParts)
      : this(new MimeHeaderCollection(), null, subParts)
    {
    }

    public MimeMessage(IEnumerable<MimeHeader> headers, IEnumerable<MimeMessage> subParts)
      : this(new MimeHeaderCollection(headers), null, subParts)
    {
    }

    public MimeMessage(MimeHeaderCollection headers, IEnumerable<MimeMessage> subParts)
      : this(headers, null, subParts)
    {
    }

    internal MimeMessage(MimeHeaderCollection headers, MemoryStream content, IEnumerable<MimeMessage> subParts)
    {
      if (headers == null)
        throw new ArgumentNullException("headers");

      this.headers = headers;
      this.Content = content;
      this.subParts = new List<MimeMessage>(subParts);

      this.Format = MimeFormat.Standard;
      this.MimeType = MimeType.MultipartMixed;
      this.Charset = Charsets.ISO8859_1;
      this.Boundary = null;
      this.TransferEncoding = ContentTransferEncodingMethod.SevenBit;
      this.Disposition = MimeMessageDisposition.None;
    }

    public static MimeMessage Load(string file)
    {
      using (var stream = File.OpenRead(file)) {
        return Load(stream);
      }
    }

    public static MimeMessage LoadMessage(string message)
    {
      return Load(new MemoryStream(Charsets.ISO8859_1.GetBytes(message)));
    }

    public static MimeMessage Load(Stream stream)
    {
      var message = Decode.Parser.Parse(stream);

#if false
      // MIME-Version
      if (message.Headers.Contains("MIME-Version") && message.Headers["MIME-Version"].Value.Trim() != "1.0")
        throw new NotSupportedException("unsupported MIME version");
#endif

      return Decode.Decoder.Decode(message);
    }

#region "Create*"
    public static MimeMessage CreateMultipart(IEnumerable<MimeMessage> subParts)
    {
      var message = new MimeMessage(subParts);

      message.Boundary = string.Format("----------------{0:D16}", DateTime.Now.Ticks);
      message.MimeType = MimeType.MultipartMixed;

      message.WriteContent("This is a multi-part message in MIME format.", ContentTransferEncodingMethod.SevenBit, Encoding.ASCII);

      return message;
    }

    public static MimeMessage CreateAttachment(string file)
    {
      return CreateAttachment(file, null, false);
    }

    public static MimeMessage CreateAttachment(string file, bool inline)
    {
      return CreateAttachment(file, null, inline);
    }

    public static MimeMessage CreateAttachment(string file, string description, bool inline)
    {
      using (var stream = File.OpenRead(file)) {
        return CreateAttachment(stream, MimeType.GetMimeTypeByExtension(file), Path.GetFileName(file), description, inline);
      }
    }

    public static MimeMessage CreateAttachment(Stream contentStream, MimeType mimeType)
    {
      return CreateAttachment(contentStream, mimeType, null, null, false);
    }

    public static MimeMessage CreateAttachment(Stream contentStream, MimeType mimeType, bool inline)
    {
      return CreateAttachment(contentStream, mimeType, null, null, inline);
    }

    public static MimeMessage CreateAttachment(Stream contentStream, MimeType mimeType, string filename, bool inline)
    {
      return CreateAttachment(contentStream, mimeType, filename, null, inline);
    }

    public static MimeMessage CreateAttachment(Stream contentStream, MimeType mimeType, string filename, string description, bool inline)
    {
      if (contentStream == null)
        throw new ArgumentNullException("contentStream");
      if (mimeType == null)
        throw new ArgumentNullException("mimeType");

      var disposition = inline ? MimeMessageDisposition.Inline : MimeMessageDisposition.Attachment;

      // TODO: Content-Type + name parameter
      var message = new MimeMessage(new[] {
        MimeHeader.CreateContentType(mimeType, null, null, filename),
        MimeHeader.CreateContentTransferEncoding(ContentTransferEncodingMethod.Base64),
        MimeHeader.CreateContentDisposition(disposition, filename),
        new MimeHeader("Content-Description", description, MimeEncodingMethod.Base64, Charsets.UTF8),
      });

      message.MimeType = mimeType;
      message.Disposition = disposition;

      message.WriteContent(ContentTransferEncodingMethod.Base64, delegate(BinaryWriter writer) {
        contentStream.CopyTo(writer);
      });

      return message;
    }
#endregion

    public void Save(string file)
    {
      using (var stream = File.OpenWrite(file)) {
        stream.SetLength(0);

        Save(stream);
      }
    }

    public void Save(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      Encode.Formatter.Format(Encode.Encoder.Encode(this), stream);
    }

#region "content reading/writing"
    /// <remarks>do not close supplied reader</remarks>
    public void ReadContent(Action<StreamReader> read)
    {
      if (read == null)
        throw new ArgumentNullException("read");

      read(Decode.Decoder.CreateContentTextReader(this));
    }

    /// <remarks>do not close supplied reader</remarks>
    public void ReadContent(Action<BinaryReader> read)
    {
      if (read == null)
        throw new ArgumentNullException("read");

      read(Decode.Decoder.CreateContentBinaryReader(this));
    }

    public string ReadContentAsText()
    {
      return Decode.Decoder.CreateContentTextReader(this).ReadToEnd();
    }

    public byte[] ReadContentAsBinary()
    {
      var contentStream = Decode.Decoder.CreateContentReadingStream(this);
      var resultStream = new MemoryStream();

      contentStream.CopyTo(resultStream);

      resultStream.Close();

      return resultStream.ToArray();
    }

    /// <remarks>do not close supplied writer</remarks>
    public void WriteContent(ContentTransferEncodingMethod transferEncoding, Encoding charset, Action<StreamWriter> write)
    {
      if (charset == null)
        throw new ArgumentNullException("charset");
      if (write == null)
        throw new ArgumentNullException("write");

      this.Charset = charset;
      this.TransferEncoding = transferEncoding;

      var writer = Encode.Encoder.CreateContentTextWriter(this);

      write(writer);

      writer.Flush();

      if (writer.BaseStream is Encode.ContentEncodingStream)
        (writer.BaseStream as Encode.ContentEncodingStream).FlushFinalBlock();
    }

    /// <remarks>do not close supplied writer</remarks>
    public void WriteContent(ContentTransferEncodingMethod transferEncoding, Action<BinaryWriter> write)
    {
      if (write == null)
        throw new ArgumentNullException("write");

      this.Charset = Charsets.ISO8859_1;
      this.TransferEncoding = transferEncoding;

      var writer = Encode.Encoder.CreateContentBinaryWriter(this);

      write(writer);

      writer.Flush();

      if (writer.BaseStream is Encode.ContentEncodingStream)
        (writer.BaseStream as Encode.ContentEncodingStream).FlushFinalBlock();
    }

    public void WriteContent(string content, ContentTransferEncodingMethod transferEncoding, Encoding charset)
    {
      WriteContent(transferEncoding, charset, delegate(StreamWriter writer) {
        writer.Write(content);
      });
    }

    public void WriteContent(byte[] content, ContentTransferEncodingMethod transferEncoding)
    {
      WriteContent(content, 0, content.Length, transferEncoding);
    }

    public void WriteContent(byte[] content, int index, int count, ContentTransferEncodingMethod transferEncoding)
    {
      WriteContent(transferEncoding, delegate(BinaryWriter writer) {
        writer.Write(content, index, count);
      });
    }
#endregion

    public override string ToString()
    {
      using (var stream = new MemoryStream(1024)) {
        Save(stream);

        stream.Close();

        return Charsets.ISO8859_1.GetString(stream.ToArray());
      }
    }

    private /*readonly*/ MimeHeaderCollection headers;
    private /*readonly*/ List<MimeMessage> subParts;
  }
}