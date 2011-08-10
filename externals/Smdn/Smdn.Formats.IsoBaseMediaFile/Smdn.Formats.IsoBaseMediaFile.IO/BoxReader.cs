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
  public class BoxReader : Smdn.IO.BigEndianBinaryReader {
    public BoxReader(Stream stream)
      : this(stream, false)
    {
    }

    internal BoxReader(Stream stream, bool leaveBaseStreamOpen)
      : base(stream, leaveBaseStreamOpen, 16)
    {
    }

    public Box ReadBox(bool readFields)
    {
      return ReadBox(null, readFields, null);
    }

    public Box ReadBox(bool readFields, Type typeOfBox)
    {
      return ReadBox(null, readFields, typeOfBox);
    }

    internal Box ReadBox(Box container, bool readFields, Type typeOfBox)
    {
      var box = InternalReadBox(container, readFields, typeOfBox);

      if (box == null)
        return null;

      if (readFields && container != null) {
        // update offset of boxes
        var containerOffset = 8UL; // size + type

        if (0 < (container.Size & 0xffffffff00000000))
          containerOffset += 8UL; // large size

        if (container is UserExtensionBox)
          containerOffset += 16UL; // uuid

        box.UpdateNestOffset(container.Offset + containerOffset);
      }

      return box;
    }

    private Box InternalReadBox(Box container, bool readFields, Type typeOfBox)
    {
      if (EndOfStream)
        return null;

      if (BaseStream.Length < BaseStream.Position + 8) // size(4) + type(4)
        throw new InvalidDataException("invalid box format");

      // read fields of Box
      var offset = (ulong)BaseStream.Position;
      var size = (ulong)ReadUInt32();
      var type = ReadFourCC();
      bool largeSize = (size == 1);

      if (largeSize) {
        if (BaseStream.Length < BaseStream.Position + 8) // large size(8)
          throw new InvalidDataException("invalid box format");

        size = ReadUInt64();

        if (size != 0 && size < 16)
          throw new InvalidDataException("invalid box size");
      }
      else {
        if (size != 0 && size < 8)
          throw new InvalidDataException("invalid box size");
      }

      Uuid? userType = null;

      if (type == Box.TypeFourCC.Uuid) {
        // read fields of UserExtensionBox
        if (BaseStream.Length < BaseStream.Position + 16) // uuid(16)
          throw new InvalidDataException("invalid user type box format");

        ReadBytesUnchecked(Storage, 0, 16, true);

        userType = new Uuid(Storage, 0, Endianness);
      }

      Box box = null;

      try {
        if (userType.HasValue) {
          // UserExtensionBox
          box = Box.Create(userType.Value, typeOfBox);

          (box as UserExtensionBox).UserType = userType.Value;
        }
        else {
          // Box
          box = Box.Create(type, typeOfBox);

          box.Type = type;
        }
      }
      catch (InvalidCastException ex) {
        if (userType.HasValue)
          throw new InvalidDataException(string.Format("the user type box '{0}' is not assignable to {1}", userType.Value, typeOfBox), ex);
        else
          throw new InvalidDataException(string.Format("the box '{0}' is not assignable to {1}", type, typeOfBox), ex);
      }

      box.Offset = offset;
      box.Size = size;

      // this value will be used before calling BoxList<>.Add, and updated by BoxList<>.Add
      box.ContainedIn = container as IBoxContainer;

      if (box is ContainerBoxBase) {
        var containerBox = box as ContainerBoxBase;
        var endOfBox = (long)(offset + size);

        while (BaseStream.Position < endOfBox) {
          containerBox.Boxes.Add(InternalReadBox(box, readFields, null));
        }
      }
      else {
        long bytesToRead;

        if (size == 0) {
          // box extends to end of stream
          bytesToRead = BaseStream.Length - BaseStream.Position;
        }
        else {
          if (largeSize)
            bytesToRead = (long)size - 16L;
          else
            bytesToRead = (long)size - 8L;

          if (userType.HasValue)
            bytesToRead -= 16L;
        }

        if (0 < bytesToRead) {
          if (readFields) {
            try {
              using (var fieldReader = new BoxFieldReader(PartialStream.CreateNonNested(BaseStream, bytesToRead, true), true)) {
                BoxFields.ReadFields(box, fieldReader);

                box.UndefinedFieldData = fieldReader.ReadRemainder();
              }
            }
            catch (InvalidDataException ex) {
              Console.Error.WriteLine("InvalidDataException occured at {0}/{1} in {2}", BaseStream.Position, BaseStream.Length, GetType().FullName);
              Console.Error.WriteLine(ex);
            }
          }
          else {
            BaseStream.Seek(bytesToRead, SeekOrigin.Current);
          }
        }
      }

      return box;
    }
  }
}
