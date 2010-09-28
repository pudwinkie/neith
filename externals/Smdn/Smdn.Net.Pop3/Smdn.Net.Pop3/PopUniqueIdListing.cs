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

namespace Smdn.Net.Pop3 {
  public struct PopUniqueIdListing :
    IEquatable<long>,
    IEquatable<string>
  {
    public static readonly PopUniqueIdListing Invalid = new PopUniqueIdListing(0L);

    public long MessageNumber {
      get; private set;
    }

    public string UniqueId {
      get; private set;
    }

    private PopUniqueIdListing(long messageNumber)
      : this()
    {
      MessageNumber = messageNumber;
      UniqueId = null;
    }

    public PopUniqueIdListing(long messageNumber, string uniqueId)
      : this()
    {
      if (uniqueId == null)
        throw new ArgumentNullException("uniqueId");
      else if (uniqueId.Length == 0)
        throw new ArgumentException("must be non-empty string", "uniqueId");

      this.MessageNumber = messageNumber;
      this.UniqueId = uniqueId;
    }

    public bool Equals(long other)
    {
      return this.MessageNumber == other;
    }

    public bool Equals(string other)
    {
      return string.Equals(this.UniqueId, other, StringComparison.Ordinal);
    }

    public override string ToString()
    {
      return string.Format("{{MessageNumber={0}, UniqueId={1}}}", MessageNumber, UniqueId);
    }
  }
}
