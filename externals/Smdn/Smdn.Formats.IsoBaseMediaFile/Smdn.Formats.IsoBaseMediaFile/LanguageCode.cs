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

namespace Smdn.Formats.IsoBaseMediaFile {
  public struct LanguageCode :
    IEquatable<LanguageCode>,
    IEquatable<short>,
    IEquatable<string>,
    IEquatable<byte[]>
  {
    public static readonly LanguageCode Empty = new LanguageCode(0);
    public static readonly LanguageCode Undetermined = new LanguageCode("und");
    public static readonly LanguageCode MultipleLanguages = new LanguageCode("mul");

    public LanguageCode(byte first5bit, byte second5bit, byte third5bit)
    {
      this.code = new byte[3];
      this.code[0] = (byte)(first5bit  & 0x1f);
      this.code[1] = (byte)(second5bit & 0x1f);
      this.code[2] = (byte)(third5bit  & 0x1f);
    }

    public LanguageCode(short langCode)
    {
      this.code = new byte[3];
      this.code[0] = (byte)((langCode & 0xe500) >> 10);
      this.code[1] = (byte)((langCode & 0x03e0) >> 5);
      this.code[2] = (byte)(langCode & 0x001f);
    }

    public LanguageCode(string lang)
    {
      if (lang == null)
        throw new ArgumentNullException("lang");
      if (lang.Length != 3)
        throw new ArgumentException("length must be 3");

      this.code = new byte[3];
      this.code[0] = (byte)((byte)lang[0] - 0x60);
      this.code[1] = (byte)((byte)lang[1] - 0x60);
      this.code[2] = (byte)((byte)lang[2] - 0x60);
    }

    public static explicit operator LanguageCode(short langCode)
    {
      return new LanguageCode(langCode);
    }

    public static explicit operator short(LanguageCode langCode)
    {
      return langCode.ToInt16();
    }

    public static implicit operator LanguageCode(string lang)
    {
      return new LanguageCode(lang);
    }

    public static explicit operator string(LanguageCode lang)
    {
      return lang.ToString();
    }

    public static bool operator ==(LanguageCode a, LanguageCode b)
    {
      return (a.Equals(b));
    }

    public static bool operator !=(LanguageCode a, LanguageCode b)
    {
      return !(a.Equals(b));
    }

    public override bool Equals(object obj)
    {
      if (obj is LanguageCode)
        return Equals((LanguageCode)obj);
      else if (obj is string)
        return Equals(obj as string);
      else if (obj is short)
        return Equals((short)obj);
      else
        return false;
    }

    public bool Equals(string lang)
    {
      return string.Equals(this.ToString(), lang);
    }

    public bool Equals(short langCode)
    {
      return (this.ToInt16() == langCode);
    }

    public bool Equals(byte[] langCode)
    {
      if (langCode.Length != 3)
        return false;

      return (code[0] == langCode[0] && code[1] == langCode[1] && code[2] == langCode[2]);
    }

    public override int GetHashCode()
    {
      return (code[0] << 10) | (code[1] << 5) | code[2];
    }

    public bool Equals(LanguageCode langCode)
    {
      return (langCode.code[0] == this.code[0] &&
              langCode.code[1] == this.code[1] &&
              langCode.code[2] == this.code[2]);
    }

    public short ToInt16()
    {
      return (short)((code[0] << 10) | (code[1] << 5) | code[2]);
    }

    public byte[] ToByteArray()
    {
      var clone = new byte[3];

      code.CopyTo(clone, 0);

      return clone;
    }

    public override string ToString()
    {
      return new string(new[] {(char)(code[0] + 0x60), (char)(code[1] + 0x60), (char)(code[2] + 0x60)});
    }

    private byte[] code;
  }
}
