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

namespace Smdn.Formats.Earthsoft.PV4 {
  // 量子化テーブル (輝度・色差)
  public sealed class QuantizerTable :
    IEquatable<QuantizerTable>,
    IEquatable<ushort[]>,
    IEquatable<short[]>
  {
    public ushort this[int index] {
      get { return table[index]; }
      set { table[index] = value; }
    }

    public ushort this[int x, int y] {
      get { return table[y * 8 + x]; }
      set { table[y * 8 + x] = value; }
    }

    public ushort[] ToArray()
    {
      var array = new ushort[64];

      Buffer.BlockCopy(table, 0, array, 0, 64 * 2);

      return array;
    }

    public override string ToString()
    {
      var sb = new System.Text.StringBuilder();

      for (var y = 0; y < 8; y++) {
        if (y != 0)
          sb.AppendLine();
        for (var x = 0; x < 8; x++) {
          sb.AppendFormat("{0:x4} ", table[y * 8 + x]);
        }
      }

      return sb.ToString();
    }

    public override bool Equals(object obj)
    {
      if (obj is QuantizerTable)
        return Equals(obj as QuantizerTable);
      else if (obj is ushort[])
        return Equals(obj as ushort[]);
      else if (obj is short[])
        return Equals(obj as short[]);
      else
        return false;
    }

    public bool Equals(QuantizerTable other)
    {
      for (var index = 0; index < 64; index++) {
        if (this.table[index] != other.table[index])
          return false;
      }

      return true;
    }

    public bool Equals(ushort[] other)
    {
      if (other.Length != 64)
        return false;

      for (var index = 0; index < 64; index++) {
        if (table[index] != other[index])
          return false;
      }

      return true;
    }

    public bool Equals(short[] other)
    {
      if (other.Length != 64)
        return false;

      for (var index = 0; index < 64; index++) {
        if ((short)table[index] != other[index])
          return false;
      }

      return true;
    }

    public override int GetHashCode()
    {
      var hashcode = 0;

      for (var index = 0; index < 64; index += 2) {
        hashcode ^= ((table[index + 1] << 16) | table[index]);
      }

      return hashcode;
    }

    private readonly ushort[] table = new ushort[64];
  }
}
