// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2010-2011 smdn
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

namespace Smdn.Net.MessageAccessProtocols {
  internal class StringEnumComparer :
    EqualityComparer<IStringEnum>,
    IEqualityComparer<string>
  {
    internal StringEnumComparer(IEqualityComparer<string> comparer)
    {
      this.comparer = comparer;
    }

    public override bool Equals(IStringEnum x, IStringEnum y)
    {
      if (x == null && y == null)
        return true;
      else if (x == null || y == null)
        return false;
      else
        return comparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(IStringEnum obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      return comparer.GetHashCode(obj.Value);
    }

    public bool Equals(string x, string y)
    {
      return comparer.Equals(x, y);
    }

    public int GetHashCode(string obj)
    {
      return comparer.GetHashCode(obj);
    }

    private IEqualityComparer<string> comparer;
  }
}
