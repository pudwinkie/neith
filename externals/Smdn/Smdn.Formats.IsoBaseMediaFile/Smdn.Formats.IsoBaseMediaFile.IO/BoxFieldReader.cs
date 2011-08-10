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

using Smdn.IO;

namespace Smdn.Formats.IsoBaseMediaFile.IO {
  public class BoxFieldReader : Smdn.IO.BigEndianBinaryReader {
    public BoxFieldReader(Stream stream)
      : base(stream, false)
    {
    }

    internal BoxFieldReader(Stream stream, bool leaveBaseStreamOpen)
      : base(stream, leaveBaseStreamOpen, 36)
    {
    }

    public byte[] ReadToNull()
    {
      CheckBitsRemainder();

      var buffer = new byte[BaseStream.Length - BaseStream.Position];
      var length = 0;

      for (length = 0; length < buffer.Length; length++) {
        buffer[length] = ReadByte();

        if (buffer[length] == 0)
          break;
      }

      if (length == buffer.Length)
        return buffer;
      else
        return Smdn.ArrayExtensions.Slice(buffer, 0, length);
    }

    public byte ReadBits(int bits)
    {
      if (bits <= 0 || 8 <= bits)
        throw ExceptionUtils.CreateArgumentMustBeInRange(1, 7, "bits", bits);

      byte result = 0x00;

      if (bitsRemainder < bits) {
        result = (byte)(bitsBuffer << (bitsRemainder + 1));
        bits -= bitsRemainder;

        bitsBuffer = base.ReadByte();
        bitsRemainder = 8;
      }

      var shift = (bitsRemainder - bits);
      var mask = (0x1 << bitsRemainder) - 1;

      result |= (byte)((bitsBuffer & mask) >> shift);

      bitsBuffer &= (byte)(mask >> bits);

      bitsRemainder -= bits;

      return result;
    }

    public override byte ReadByte()
    {
      CheckBitsRemainder();

      return base.ReadByte();
    }

    public override sbyte ReadSByte()
    {
      CheckBitsRemainder();

      return base.ReadSByte();
    }

    public override short ReadInt16()
    {
      CheckBitsRemainder();

      return base.ReadInt16();
    }

    public override ushort ReadUInt16()
    {
      CheckBitsRemainder();

      return base.ReadUInt16();
    }

    public override int ReadInt32()
    {
      CheckBitsRemainder();

      return base.ReadInt32();
    }

    public override uint ReadUInt32()
    {
      CheckBitsRemainder();

      return base.ReadUInt32();
    }

    public override long ReadInt64()
    {
      CheckBitsRemainder();

      return base.ReadInt64();
    }

    public override ulong ReadUInt64()
    {
      CheckBitsRemainder();

      return base.ReadUInt64();
    }

    public override UInt24 ReadUInt24()
    {
      CheckBitsRemainder();

      return base.ReadUInt24();
    }

    public override UInt48 ReadUInt48()
    {
      CheckBitsRemainder();

      return base.ReadUInt48();
    }

    public override FourCC ReadFourCC()
    {
      CheckBitsRemainder();

      return base.ReadFourCC();
    }

    private void CheckBitsRemainder()
    {
      if (bitsRemainder != 0)
        throw new InvalidOperationException("read bits exists");
    }

    private int bitsRemainder = 0;
    private byte bitsBuffer = 0;

    public LanguageCode ReadLanguageCode()
    {
      return new LanguageCode(ReadBits(5), ReadBits(5), ReadBits(5));
    }

    public Matrix ReadMatrix()
    {
      CheckBitsRemainder();

      ReadBytesUnchecked(Storage, 0, 36, true);

      return BinaryConvert.ToMatrix(Storage, 0);
    }

    public Rgba ReadRgba()
    {
      return new Rgba(ReadUInt32());
    }

    public Uuid ReadUuid()
    {
      CheckBitsRemainder();

      ReadBytesUnchecked(Storage, 0, 16, true);

      return new Uuid(Storage, 0, Endianness);
    }

    public decimal ReadFixedPointSigned88()
    {
      CheckBitsRemainder();

      ReadBytesUnchecked(Storage, 0, 2, true);

      return BinaryConvert.ToFixedPointSigned88(Storage, 0);
    }

    public decimal ReadFixedPointSigned1616()
    {
      CheckBitsRemainder();

      ReadBytesUnchecked(Storage, 0, 4, true);

      return BinaryConvert.ToFixedPointSigned1616(Storage, 0);
    }

    public decimal ReadFixedPointUnsigned88()
    {
      CheckBitsRemainder();

      ReadBytesUnchecked(Storage, 0, 2, true);

      return BinaryConvert.ToFixedPointUnsigned88(Storage, 0);
    }

    public decimal ReadFixedPointUnsigned1616()
    {
      CheckBitsRemainder();

      ReadBytesUnchecked(Storage, 0, 4, true);

      return BinaryConvert.ToFixedPointUnsigned1616(Storage, 0);
    }

    public string ReadStringUTF8()
    {
      var str = ReadToEnd();

      if (str.Length == 0)
        return string.Empty;
      else
        return Encoding.UTF8.GetString(str);
    }

    public string ReadStringUTF8NullTerminated()
    {
      var str = ReadToNull();

      if (str.Length == 0)
        return string.Empty;
      else
        return Encoding.UTF8.GetString(str);
    }

    private string DecodeAsUnicodeString(byte[] str)
    {
      if (str.Length == 0)
        return string.Empty;

      if (ArrayExtensions.EqualsAll(str, Encoding.BigEndianUnicode.GetPreamble()))
        // UTF-16
        return Encoding.BigEndianUnicode.GetString(str);
      else
        // UTF-8
        return Encoding.UTF8.GetString(str);
    }

    public string ReadStringUnicode()
    {
      return DecodeAsUnicodeString(ReadToEnd());
    }

    public string ReadStringLengthStored8()
    {
      return ReadStringLengthStored8(0);
    }

    public string ReadStringLengthStored8(byte fieldSize)
    {
      var length = (long)ReadByte();
      var str = ReadBytes(length);

      if (0 < fieldSize)
        // skip padding
        BaseStream.Seek(fieldSize - (length + 1), SeekOrigin.Current);

      return DecodeAsUnicodeString(str);
    }

    public string ReadStringLengthStored16()
    {
      return ReadStringLengthStored16(0);
    }

    public string ReadStringLengthStored16(ushort fieldSize)
    {
      var length = (long)ReadUInt16();
      var str = ReadBytes(length);

      if (0 < fieldSize)
        // skip padding
        BaseStream.Seek(fieldSize - (length + 1), SeekOrigin.Current);

      return DecodeAsUnicodeString(str);
    }

    public DateTime ReadIsoDateTime32()
    {
      return Iso14496TimeStamp.ToDateTime(ReadUInt32());
    }

    public DateTime ReadIsoDateTime64()
    {
      return Iso14496TimeStamp.ToDateTime(ReadUInt64());
    }

    public DataBlock ReadDataBlock()
    {
      return ReadDataBlock(BaseStream.Length - BaseStream.Position);
    }

    public DataBlock ReadDataBlock(long length)
    {
      if (length < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("length", length);

      if (0 == length)
        return null;

      var ret = new DataBlock(PartialStream.CreateNonNested(BaseStream, length, false));

      BaseStream.Seek(length, SeekOrigin.Current);

      return ret;
    }

    public Box ReadBox(Box container, Type boxType)
    {
      using (var reader = new BoxReader(BaseStream, true)) {
        return reader.ReadBox(container, true, (boxType == typeof(Box)) ? null : boxType);
      }
    }

    public DataBlock ReadRemainder()
    {
      return ReadDataBlock();
    }

    internal object ReadField(FieldHandlingContext context)
    {
      if (BaseStream.Length - BaseStream.Position < (context.FieldSize + 7) / 8)
        throw new InvalidDataException(string.Format("reached the end of the stream while reading. {0}", context));

      try {
        if (context.FieldDataType == FieldDataType.Default)
          return ReadFieldWithoutType(context);
        else
          return ReadFieldWithType(context);
      }
      catch (InvalidCastException ex) {
        throw new InvalidDataException(string.Format("invalid field type. {0}", context), ex);
      }
      catch (NotSupportedException ex) {
        throw ex;
      }
      catch (Exception ex) {
        throw new InvalidDataException(string.Format("exception occured while reading. {0}", context), ex);
      }
    }

    private object ReadFieldWithType(FieldHandlingContext context)
    {
      switch (context.FieldDataType) {
        case FieldDataType.IsoDateTime:
          if (context.FieldSize == 64)
            return ReadIsoDateTime64();
          else if (context.FieldSize == 32)
            return ReadIsoDateTime32();
          else
            throw HandleUnsupportedField("field size must be 32 or 64", context);

        case FieldDataType.FixedPointSigned88:
          if (context.FieldSize == 16)
            return ReadFixedPointSigned88();
          else
            throw HandleUnsupportedField("field size must be 16", context);

        case FieldDataType.FixedPointSigned1616:
          if (context.FieldSize == 32)
            return ReadFixedPointSigned1616();
          else
            throw HandleUnsupportedField("field size must be 32", context);

        case FieldDataType.FixedPointUnsigned88:
          if (context.FieldSize != 16)
            return ReadFixedPointUnsigned88();
          else
            throw HandleUnsupportedField("field size must be 16", context);

        case FieldDataType.FixedPointUnsigned1616:
          if (context.FieldSize == 32)
            return ReadFixedPointUnsigned1616();
          else
            throw HandleUnsupportedField("field size must be 32", context);

        case FieldDataType.StringUTF8:
          return ReadStringUTF8();

        case FieldDataType.StringUTF8NullTerminated:
          return ReadStringUTF8NullTerminated();

        case FieldDataType.StringUnicode:
          return ReadStringUnicode();

        case FieldDataType.StringLengthStored8:
          return ReadStringLengthStored8((byte)(context.FieldSize / 8));

        case FieldDataType.StringLengthStored16:
          return ReadStringLengthStored16((ushort)(context.FieldSize / 8));

        default:
          throw HandleUnsupportedField("unsupported field type.", context);
      }
    }

    private object ReadFieldWithoutType(FieldHandlingContext context)
    {
      if (context.FieldType == typeof(DataBlock)) {
        if (context.FieldSize == 0)
          return ReadDataBlock();
        else
          return ReadDataBlock(context.FieldSize / 8);
      }
      else if (context.FieldSize == 0 && typeof(Box).IsAssignableFrom(context.FieldType)) {
        return ReadBox((context.Instance is Box) ? context.Instance as Box : null, context.FieldType);
      }

      switch (context.FieldSize) {
        case 1: case 2: case 3: case 4: case 5: case 6: case 7: {
          return Convert.ChangeType(ReadBits(context.FieldSize), context.FieldType);
        }

        case 8: {
          if (context.FieldType == typeof(byte))
            return ReadByte();
          else if (context.FieldType == typeof(sbyte))
            return ReadSByte();
          else
            return Convert.ChangeType(ReadByte(), context.FieldType);
        }

        case 15: {
          if (context.FieldType == typeof(LanguageCode))
            return ReadLanguageCode();
          else
            break;
        }

        case 16: {
          if (context.FieldType == typeof(ushort))
            return ReadUInt16();
          else if (context.FieldType == typeof(short))
            return ReadInt16();
          else
            return Convert.ChangeType(ReadInt16(), context.FieldType);
        }

        case 24: {
          var val = ReadUInt24();

          if (context.FieldType == typeof(UInt24))
            return val;
          else if (context.FieldType == typeof(int))
            return (int)val;
          else if (context.FieldType == typeof(uint))
            return (uint)val;
          else
            return Convert.ChangeType(val, context.FieldType);
        }

        case 32: {
          if (context.FieldType == typeof(FourCC))
            return ReadFourCC();
          else if (context.FieldType == typeof(Rgba))
            return ReadRgba();
          else if (context.FieldType == typeof(uint))
            return ReadUInt32();
          else if (context.FieldType == typeof(int))
            return ReadInt32();
          else
            return Convert.ChangeType(ReadInt32(), context.FieldType);
        }

        case 48: {
          var val = ReadUInt48();

          if (context.FieldType == typeof(UInt48))
            return val;
          else if (context.FieldType == typeof(long))
            return (long)val;
          else if (context.FieldType == typeof(ulong))
            return (ulong)val;
          else
            return Convert.ChangeType(val, context.FieldType);
        }

        case 64: {
          if (context.FieldType == typeof(ulong))
            return ReadUInt64();
          else if (context.FieldType == typeof(long))
            return ReadInt64();
          else
            return Convert.ChangeType(ReadInt64(), context.FieldType);
        }

        case 128: {
          if (context.FieldType == typeof(Uuid))
            return ReadUuid();
          else if (context.FieldType == typeof(Guid))
            return (Guid)ReadUuid();
          else
            break;
        }

        case 288: {
          if (context.FieldType == typeof(Matrix))
            return ReadMatrix();
          else
            break;
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
