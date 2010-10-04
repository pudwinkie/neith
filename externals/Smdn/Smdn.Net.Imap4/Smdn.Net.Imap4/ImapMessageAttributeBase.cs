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
using System.Text;

namespace Smdn.Net.Imap4 {
  public abstract class ImapMessageAttributeBase {
    public long Sequence {
      get; private set;
    }

    internal protected ImapMessageAttributeBase(long sequence)
    {
      if (sequence <= 0L)
        throw new ArgumentOutOfRangeException("sequence", sequence, "must be non-zero positive number");

      this.Sequence = sequence;
    }

    protected IImapUrlSearchQuery GetSequenceNumberSearchCriteria()
    {
      return new SequenseNumberSearchCriteria(Sequence);
    }

    private class SequenseNumberSearchCriteria : IImapUrlSearchQuery {
      public SequenseNumberSearchCriteria(long sequence)
      {
        this.sequence = sequence;
      }

      public byte[] GetEncodedQuery(Encoding charset, out bool charsetSpecified)
      {
        charsetSpecified = false;

        return NetworkTransferEncoding.Transfer7Bit.GetBytes(sequence.ToString());
      }

      private readonly long sequence;
    }
  }
}
