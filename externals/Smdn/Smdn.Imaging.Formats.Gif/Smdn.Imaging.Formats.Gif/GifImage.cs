// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2011 smdn
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Smdn.IO;

namespace Smdn.Imaging.Formats.Gif {
  // http://d.hatena.ne.jp/juntk/20100310/1268176738
  internal class GifImage : IDisposable {
    public static readonly ByteString GifSignature = ByteString.CreateImmutable("GIF");
    public static readonly ByteString GifVersionGIF87a = ByteString.CreateImmutable("87a");
    public static readonly ByteString GifVersionGIF89a = ByteString.CreateImmutable("89a");

    private byte[] bufferLogicalScreenDescriptor;
    private byte[] bufferColorTable;
    private byte[] bufferImageDescriptor;
    private Stream streamImageData;

    public GifImage(Image image)
    {
      using (var stream = new ChunkedMemoryStream()) {
        image.Save(stream, ImageFormat.Gif);

        stream.Position = 0L;

        Parse(stream);
      }
    }

    public void Dispose()
    {
      if (streamImageData != null) {
        streamImageData.Close();
        streamImageData = null;
      }
    }

    private void Parse(Stream gifStream)
    {
      var reader = new LittleEndianBinaryReader(gifStream, true);

      if (!GifSignature.Equals(reader.ReadExactBytes(3)))
        throw new InvalidDataException("stream is not GIF");

      var gifVersion = ByteString.CreateImmutable(reader.ReadExactBytes(3));

      if (!GifVersionGIF87a.Equals(gifVersion) &&
          !GifVersionGIF89a.Equals(gifVersion))
        throw new NotSupportedException(string.Format("unsupported GIF version : {0}", gifVersion));

      ParseLogicalScreenDescriptor(reader);

      for (;;) {
        var blockType = (GifBlockType)reader.ReadByte();

        switch (blockType) {
          case GifBlockType.ImageBlock:
            ParseImageDescriptor(reader);
            break;

          case GifBlockType.Extension:
            SkipExtensionBlock(reader);
            break;

          case GifBlockType.Trailer:
            goto parsed;

          default:
            throw new InvalidDataException(string.Format("unknown GIF block type: {0}", blockType));
        }
      }

    parsed:
        ;
    }

    private void ParseLogicalScreenDescriptor(Smdn.IO.BinaryReader reader)
    {
      const int posPackedFields = 0x04;

      bufferLogicalScreenDescriptor = reader.ReadExactBytes(7);

      if ((bufferLogicalScreenDescriptor[posPackedFields] & 0x80) == 0)
        bufferColorTable = null;
      else
        // if Global Color Table Flag is set, Global Color Table follows this block
        bufferColorTable = ReadColorTable(reader, 1 + (bufferLogicalScreenDescriptor[posPackedFields] & 0x07));

      bufferLogicalScreenDescriptor[posPackedFields] &= 0x7f; // unset Global Color Table Flag
    }

    private void ParseImageDescriptor(Smdn.IO.BinaryReader reader)
    {
      bufferImageDescriptor = new byte[10];
      bufferImageDescriptor[0] = (byte)GifBlockType.ImageBlock;

      reader.ReadExactBytes(bufferImageDescriptor, 1, 9);

      if ((bufferImageDescriptor[9] & 0x80) == 0) {
        bufferImageDescriptor[9] &= 0xf8;
        bufferImageDescriptor[9] |= (byte)(bufferLogicalScreenDescriptor[0x04] & 0x07);
      }
      else {
        // read Local Color Table if exists
        bufferColorTable = ReadColorTable(reader, 1 + (bufferImageDescriptor[9] & 0x07));
      }

      bufferImageDescriptor[9] |= 0x80;

      streamImageData = ReadImageData(reader);
    }

    private byte[] ReadColorTable(Smdn.IO.BinaryReader reader, int colorResolution)
    {
      var length = 3 * (1 << colorResolution);

      return reader.ReadExactBytes(length);
    }

    private Stream ReadImageData(Smdn.IO.BinaryReader reader)
    {
      var data = new MemoryStream();

      data.WriteByte(reader.ReadByte());

      var len = reader.ReadByte();

      for (;;) {
        data.WriteByte(len);

        if (len == 0x00)
          break;

        data.Write(reader.ReadExactBytes(len), 0, len);

        len = reader.ReadByte();
      }

      return data;
    }

    private void SkipExtensionBlock(Smdn.IO.BinaryReader reader)
    {
      reader.BaseStream.Seek(1, SeekOrigin.Current); // skip ExtensionDetermination

      for (;;) {
        var len = reader.ReadByte();

        if (len == 0)
          break;

        reader.BaseStream.Seek(len, SeekOrigin.Current);
      }
    }

    public void WriteLogicalScreenDescriptor(Stream stream)
    {
      stream.Write(bufferLogicalScreenDescriptor, 0, bufferLogicalScreenDescriptor.Length);
    }

    public void WriteColorTable(Stream stream)
    {
      if (bufferColorTable == null || bufferColorTable.Length == 0)
        return;

      stream.Write(bufferColorTable, 0, bufferColorTable.Length);
    }

    public void WriteImageDescriptor(Stream stream)
    {
      stream.Write(bufferImageDescriptor, 0, bufferImageDescriptor.Length);
    }

    public void WriteImageData(Stream stream)
    {
      streamImageData.Position = 0L;

      streamImageData.CopyTo(stream);
    }
  }
}

