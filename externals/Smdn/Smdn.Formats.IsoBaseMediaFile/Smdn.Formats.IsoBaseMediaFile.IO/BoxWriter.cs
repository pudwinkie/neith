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
using System.IO;

using Smdn.IO;

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  public class BoxWriter : Smdn.IO.BigEndianBinaryWriter {
    public BoxWriter(Stream stream)
      : this(stream, false)
    {
    }

    internal BoxWriter(Stream stream, bool leaveBaseStreamOpen)
      : base(stream, leaveBaseStreamOpen, 16)
    {
    }

    public void Write(IEnumerable<Box> boxes)
    {
      foreach (var box in boxes) {
        Write(box);
      }
    }

    public void Write(Box box)
    {
      if (box == null)
        throw new ArgumentNullException("box");

      var capacity = (int)((box.Size >> 10) + 1) << 10; // n * 1024 for initial capacity

      using (var fieldStream = (capacity < 40960) ? (Stream)new MemoryStream(capacity) : (Stream)new ChunkedMemoryStream()) {
        // write fields and containing boxes first
        if (box is ContainerBoxBase) {
          using (var boxWriter = new BoxWriter(fieldStream, true)) {
            foreach (var containedBox in (box as ContainerBoxBase).Boxes) {
              boxWriter.Write(containedBox);
            }
          }
        }
        else {
          using (var fieldWriter = new BoxFieldWriter(fieldStream, true)) {
            BoxFields.WriteFields(box, fieldWriter);

            fieldWriter.Write(box.UndefinedFieldData);
          }
        }

        // calculate box size
        var boxSize = (ulong)fieldStream.Length + 8UL; // size(4) + type(4)
        var largeSize = (0x0000000100000000 <= boxSize);

        // write size, type, large size
        if (largeSize) {
          BinaryConvert.GetBytes((uint)1, this.Endianness, Storage, 0);

          box.Type.GetBytes(Storage, 4);

          BinaryConvert.GetBytes(boxSize, this.Endianness, Storage, 8);

          WriteUnchecked(Storage, 0, 16);
        }
        else {
          BinaryConvert.GetBytes((uint)boxSize, this.Endianness, Storage, 0);

          box.Type.GetBytes(Storage, 4);

          WriteUnchecked(Storage, 0, 8);
        }

        // write fields
        var s = fieldStream as MemoryStream;

        if (s == null) {
          fieldStream.Position = 0L;
          fieldStream.CopyTo(BaseStream);
        }
        else {
          WriteUnchecked(s.GetBuffer(), 0, (int)s.Length);
        }
      }
    }
  }
}