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
using System.IO;

namespace Smdn.Imaging.Formats.Gif {
  // http://bloggingabout.net/blogs/rick/archive/2005/05/10/3830.aspx
  // http://en.wikipedia.org/wiki/Graphics_Interchange_Format
  public class AnimatedGifWriter : Smdn.IO.LittleEndianBinaryWriter {
    private const int maxStorageLength = 19;

    public AnimatedGifWriter(Stream stream)
      : this(stream, false, 0)
    {
    }

    
    public AnimatedGifWriter(Stream stream, bool leaveBaseStreamOpen)
      : this(stream, leaveBaseStreamOpen, 0)
    {
    }

    public AnimatedGifWriter(Stream stream, int repetition)
      : this(stream, false, repetition)
    {
    }

    public AnimatedGifWriter(Stream stream, bool leaveBaseStreamOpen, int repetition)
      : base(stream, leaveBaseStreamOpen, maxStorageLength)
    {
      if (repetition < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("repetition", repetition);

      this.repetition = repetition;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing) {
        if (!trailerWritten) {
          Write((byte)GifBlockType.Trailer);
          trailerWritten = true;
        }
      }

      base.Dispose(disposing);
    }

    public void WriteImage(Image image, TimeSpan delay)
    {
      WriteImage(image, (int)delay.TotalMilliseconds);
    }

    public void WriteImage(Image image, int millisecondsDelay)
    {
      if (image == null)
        throw new ArgumentNullException("image");
      if (millisecondsDelay < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("millisecondsDelay", millisecondsDelay);

      using (var gifImage = new GifImage(image)) {
        if (!headerWritten) {
          WriteHeader(gifImage);
          headerWritten = true;
        }

        var delay = (uint)(millisecondsDelay + 9) / 10;

        Storage[0] = (byte)GifBlockType.Extension;
        Storage[1] = (byte)GifExtensionBlockType.GraphicControlExtension;
        Storage[2] = (byte)0x04; // Block Size
        Storage[3] = (byte)0x00; // Reserved(3bits),
                                 // Disposal Method(3bit),
                                 // User Input Flag (1bit),
                                 // Transparent Color Flag (1bit)
        Storage[4] = (byte)(delay & 0xff); // Delay Time (low)
        Storage[5] = (byte)(delay >> 8); // Delay Time (high)
        Storage[6] = (byte)0x00; // Transparent Color Index
        Storage[7] = (byte)0x00; // Block Terminator

        WriteUnchecked(Storage, 0, 8);

        gifImage.WriteImageDescriptor(BaseStream);
        gifImage.WriteColorTable(BaseStream);
        gifImage.WriteImageData(BaseStream);
      }
    }

    private void WriteHeader(GifImage gifImage)
    {
      Write(GifImage.GifSignature.Segment);

      Write(GifImage.GifVersionGIF89a.Segment);

      gifImage.WriteLogicalScreenDescriptor(BaseStream);

      Storage[ 0] = (byte)GifBlockType.Extension;
      Storage[ 1] = (byte)GifExtensionBlockType.ApplicationExtension;
      Storage[ 2] = (byte)0x0b; // Size of DataBlock (11) for NETSCAPE2.0)
      Storage[ 3] = (byte)'N';
      Storage[ 4] = (byte)'E';
      Storage[ 5] = (byte)'T';
      Storage[ 6] = (byte)'S';
      Storage[ 7] = (byte)'C';
      Storage[ 8] = (byte)'A';
      Storage[ 9] = (byte)'P';
      Storage[10] = (byte)'E';
      Storage[11] = (byte)'2';
      Storage[12] = (byte)'.';
      Storage[13] = (byte)'0';
      Storage[14] = (byte)0x03; // Size of Loop Block
      Storage[15] = (byte)0x01; // Loop Indicator
      Storage[16] = (byte)((uint)repetition & 0xff); // Number of repetitions
      Storage[17] = (byte)((uint)repetition >> 8); // 0 for endless loop
      Storage[18] = (byte)0x00; // Block Terminator

      WriteUnchecked(Storage, 0, 19);
    }

    private readonly int repetition;
    private bool headerWritten = false;
    private bool trailerWritten = false;
  }
}

