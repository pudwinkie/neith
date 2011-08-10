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
using System.Text;

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  public class BoxFieldWriter : Smdn.IO.BigEndianBinaryWriter {
    public BoxFieldWriter(Stream stream)
      : this(stream, false)
    {
    }

    internal BoxFieldWriter(Stream stream, bool leaveBaseStreamOpen)
      : base(stream, leaveBaseStreamOpen, 36)
    {
    }

    public void WriteBits(byte data, int bits)
    {
      if (bits <= 0 || 8 <= bits)
        throw ExceptionUtils.CreateArgumentMustBeInRange(1, 7, "bits", bits);

      var filled = (8 <= bits + unwrittenBits);
      var shift = filled ? (bits + unwrittenBits) - 8 : bits;
      var masked = (byte)(data & ((0x1 << shift) - 1));

      if (filled) {
        bitsBuffer <<= (8 - unwrittenBits);
        bitsBuffer |= (byte)(data >> shift);

        base.Write(bitsBuffer);

        unwrittenBits = shift;
        bitsBuffer = masked;
      }
      else {
        bitsBuffer <<= shift;

        unwrittenBits += shift;
        bitsBuffer |= masked;
      }
    }

    public override void Write(byte @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(sbyte @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(short @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(ushort @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(int @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(uint @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(long @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(ulong @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(UInt24 @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(UInt48 @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    public override void Write(FourCC @value)
    {
      CheckUnwritten();

      base.Write(@value);
    }

    private void CheckUnwritten()
    {
      if (unwrittenBits != 0)
        throw new InvalidOperationException("written bits exists");
    }

    private int unwrittenBits = 0;
    private byte bitsBuffer = 0;

    public void Write(LanguageCode langCode)
    {
      var arr = langCode.ToByteArray();

      WriteBits(arr[0], 5);
      WriteBits(arr[1], 5);
      WriteBits(arr[2], 5);
    }

    public void Write(Matrix matrix)
    {
      BinaryConvert.GetBytes(matrix, Storage, 0);

      WriteUnchecked(Storage, 0, 36);
    }

    public void Write(Rgba rgba)
    {
      Write((uint)rgba);
    }

    public void Write(Uuid uuid)
    {
      uuid.GetBytes(Storage, 0, Endianness);

      WriteUnchecked(Storage, 0, 16);
    }

    public void WriteFixedPointSigned88(decimal @value)
    {
      BinaryConvert.GetBytesFixedPointSigned88(@value, Storage, 0);

      WriteUnchecked(Storage, 0, 2);
    }

    public void WriteFixedPointSigned1616(decimal @value)
    {
      BinaryConvert.GetBytesFixedPointSigned1616(@value, Storage, 0);

      WriteUnchecked(Storage, 0, 4);
    }

    public void WriteFixedPointUnsigned88(decimal @value)
    {
      BinaryConvert.GetBytesFixedPointUnsigned88(@value, Storage, 0);

      WriteUnchecked(Storage, 0, 2);
    }

    public void WriteFixedPointUnsigned1616(decimal @value)
    {
      BinaryConvert.GetBytesFixedPointUnsigned1616(@value, Storage, 0);

      WriteUnchecked(Storage, 0, 4);
    }

    public void WriteStringUTF8(string str)
    {
      if (str == null)
        return;

      Write(Encoding.UTF8.GetBytes(str));
    }

    public void WriteStringUTF8NullTerminated(string str)
    {
      WriteStringUTF8(str);
      Write((byte)0);
    }

    public void WriteStringUnicode(string str)
    {
      if (str == null)
        return;

      Write(Encoding.BigEndianUnicode.GetPreamble());
      Write(Encoding.BigEndianUnicode.GetBytes(str));
    }

    public void WriteStringLengthStored8(string str)
    {
      WriteStringLengthStored8(str, 0);
    }

    public void WriteStringLengthStored8(string str, byte fieldSize)
    {
      if (str == null) {
        Write((byte)0);
        return;
      }

      var count = Encoding.UTF8.GetByteCount(str);

      if (0xff < count)
        throw new ArgumentException("bytes of string must be less than 256");

      var buffer = new byte[(0 < fieldSize) ? fieldSize : count + 1];

      buffer[0] = (byte)count;

      Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 1);

      Write(buffer);
    }

    public void WriteStringLengthStored16(string str, ushort fieldSize)
    {
      if (str == null)
        str = string.Empty;

      var count = Encoding.UTF8.GetByteCount(str);

      if (0xffff < count)
        throw new ArgumentException("bytes of string must be less than 65536");

      var buffer = new byte[(0 < fieldSize) ? fieldSize : count + 2];

      BinaryConvert.GetBytes((ushort)count, this.Endianness, buffer, 0);

      Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 2);

      Write(buffer);
    }

    public void WriteIsoDateTime32(DateTime dateTime)
    {
      Write(Iso14496TimeStamp.ToUInt32(dateTime));
    }

    public void WriteIsoDateTime64(DateTime dateTime)
    {
      Write(Iso14496TimeStamp.ToUInt64(dateTime));
    }

    public void Write(DataBlock block)
    {
      if (block == null)
        return;

      Write(block, block.Length);
    }

    public void Write(DataBlock block, long length)
    {
      if (length < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("length", length);

      if (length == 0)
        return;
      if (block == null)
        return;

      using (var stream = block.OpenRead()) {
        var buffer = new byte[Math.Min(0x400, length)];

        while (0 < length) {
          var read = stream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));

          if (read == 0)
            break;

          BaseStream.Write(buffer, 0, read);

          length -= read;
        }
      }
    }

    public void Write(Box box)
    {
      using (var writer = new BoxWriter(BaseStream, true)) {
        writer.Write(box);
      }
    }

    internal void WriteField(FieldHandlingContext context)
    {
      try {
        if (context.FieldDataType == FieldDataType.Default)
          WriteFieldWithoutType(context);
        else
          WriteFieldWithType(context);
      }
      catch (InvalidCastException ex) {
        throw new InvalidDataException(string.Format("invalid field type. {0}", context), ex);
      }
      catch (NotSupportedException ex) {
          throw ex;
      }
      catch (Exception ex) {
        throw new InvalidDataException(string.Format("exception occured while writing. {0}", context), ex);
      }
    }

    private void WriteFieldWithType(FieldHandlingContext context)
    {
      switch (context.FieldDataType) {
        case FieldDataType.IsoDateTime:
          if (context.FieldSize == 64) {
            WriteIsoDateTime64((DateTime)context.GetValue());
            return;
          }
          else if (context.FieldSize == 32) {
            WriteIsoDateTime32((DateTime)context.GetValue());
            return;
          }
          else {
            throw HandleUnsupportedField("field size must be 32 or 64", context);
          }

        case FieldDataType.FixedPointSigned88:
          if (context.FieldSize == 16) {
            WriteFixedPointSigned88((decimal)context.GetValue());
            return;
          }
          else {
            throw HandleUnsupportedField("field size must be 16", context);
          }

        case FieldDataType.FixedPointSigned1616:
          if (context.FieldSize == 32) {
            WriteFixedPointSigned1616((decimal)context.GetValue());
            return;
          }
          else {
            throw HandleUnsupportedField("field size must be 32", context);
          }

        case FieldDataType.FixedPointUnsigned88:
          if (context.FieldSize == 16) {
            WriteFixedPointUnsigned88((decimal)context.GetValue());
            return;
          }
          else {
            throw HandleUnsupportedField("field size must be 16", context);
          }

        case FieldDataType.FixedPointUnsigned1616:
          if (context.FieldSize == 32) {
            WriteFixedPointUnsigned1616((decimal)context.GetValue());
            return;
          }
          else {
            throw HandleUnsupportedField("field size must be 32", context);
          }

        case FieldDataType.StringUTF8:
          WriteStringUTF8(context.GetValue() as string);
          return;

        case FieldDataType.StringUTF8NullTerminated:
          WriteStringUTF8NullTerminated(context.GetValue() as string);
          return;

        case FieldDataType.StringUnicode:
          // TODO: UTF16 or UTF8?
          WriteStringUnicode(context.GetValue() as string);
          return;

        case FieldDataType.StringLengthStored8:
          WriteStringLengthStored8(context.GetValue() as string, (byte)(context.FieldSize / 8));
          return;

        case FieldDataType.StringLengthStored16:
          WriteStringLengthStored16(context.GetValue() as string, (ushort)(context.FieldSize / 8));
          return;

        default:
          throw HandleUnsupportedField("unsupported field type.", context);
      }
    }

    private void WriteFieldWithoutType(FieldHandlingContext context)
    {
      if (context.FieldType == typeof(DataBlock)) {
        if (context.FieldSize == 0)
          Write(context.GetValue() as DataBlock);
        else
          Write(context.GetValue() as DataBlock, context.FieldSize / 8);
        return;
      }
      else if (context.FieldSize == 0 && typeof(Box).IsAssignableFrom(context.FieldType)) {
        var box = context.GetValue() as Box;
        if (box != null)
          Write(box);
        return;
      }

      switch (context.FieldSize) {
        case 1: case 2: case 3: case 4: case 5: case 6: case 7: {
          WriteBits((byte)Convert.ChangeType(context.GetValue(), typeof(byte)), context.FieldSize);
          return;
        }

        case 8: {
          if (context.FieldType == typeof(byte))
            Write((byte)context.GetValue());
          else if (context.FieldType == typeof(sbyte))
            Write((sbyte)context.GetValue());
          else
            Write((byte)Convert.ChangeType(context.GetValue(), typeof(byte)));
          return;
        }

        case 15: {
          if (context.FieldType == typeof(LanguageCode)) {
            Write((LanguageCode)context.GetValue());
            return;
          }
          else {
            break;
          }
        }

        case 16: {
          if (context.FieldType == typeof(ushort))
            Write((ushort)context.GetValue());
          else if (context.FieldType == typeof(short))
            Write((short)context.GetValue());
          else
            Write((ushort)Convert.ChangeType(context.GetValue(), typeof(ushort)));
          return;
        }

        case 24: {
          Write((UInt24)((int)Convert.ChangeType(context.GetValue(), typeof(int)) & 0xffffff));
          return;
        }

        case 32: {
          if (context.FieldType == typeof(FourCC))
            Write((FourCC)context.GetValue());
          else if (context.FieldType == typeof(Rgba))
            Write((Rgba)context.GetValue());
          else if (context.FieldType == typeof(uint))
            Write((uint)context.GetValue());
          else if (context.FieldType == typeof(int))
            Write((int)context.GetValue());
          else
            Write((uint)Convert.ChangeType(context.GetValue(), typeof(uint)));
          return;
        }

        case 48: {
          Write((UInt48)((long)Convert.ChangeType(context.GetValue(), typeof(long)) & 0xffffffffffff));
          return;
        }

        case 64: {
          if (context.FieldType == typeof(ulong))
            Write((ulong)context.GetValue());
          else if (context.FieldType == typeof(long))
            Write((long)context.GetValue());
          else
            Write((ulong)Convert.ChangeType(context.GetValue(), typeof(ulong)));
          return;
        }

        case 128: {
          if (context.FieldType == typeof(Uuid) || context.FieldType == typeof(Guid)) {
            Write((Uuid)context.GetValue());
            return;
          }
          else {
            break;
          }
        }

        case 288: {
          if (context.FieldType == typeof(Matrix)) {
            Write((Matrix)context.GetValue());
            return;
          }
          else {
            break;
          }
        }
      }

      throw HandleUnsupportedField("unsupported field type.", context);
    }

    private Exception HandleUnsupportedField(string message, FieldHandlingContext context)
    {
      return new NotSupportedException(string.Format("{0} {1}", message, context));
    }
  }
}