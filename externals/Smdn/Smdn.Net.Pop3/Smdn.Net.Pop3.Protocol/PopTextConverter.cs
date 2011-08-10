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

namespace Smdn.Net.Pop3.Protocol {
  public static class PopTextConverter {
    public static PopCapability ToCapability(ByteString[] texts)
    {
      /*
       * capability   =  capa-tag *(SP param) CRLF   ;512 octets maximum
       */
      ThrowIfTooFewTexts(texts, 1);

      var tag = texts[0].ToString();
      var args = (texts.Length == 1)
        ? new string[0]
        : Array.ConvertAll<ByteString, string>(texts.Slice(1), ToString);

      return PopCapability.GetKnownOrCreate(tag, args);
    }

    public static PopDropListing ToDropListing(ByteString[] texts)
    {
      ThrowIfTooFewTexts(texts, 2);

      return new PopDropListing(ToNumber(texts[0]),
                                ToNumber(texts[1]));
    }

    public static PopScanListing ToScanListing(ByteString[] texts)
    {
      ThrowIfTooFewTexts(texts, 2);

      return new PopScanListing(ToMessageNumber(texts[0]),
                                ToNumber(texts[1]));
    }

    public static PopUniqueIdListing ToUniqueIdListing(ByteString[] texts)
    {
      ThrowIfTooFewTexts(texts, 2);

      return new PopUniqueIdListing(ToMessageNumber(texts[0]),
                                    ToString(texts[1]));
    }

    public static long ToMessageNumber(string messageNumber)
    {
      var val = ToNumber(messageNumber);

      if (val == 0L)
        throw new PopMalformedTextException(string.Format("must be non-zero positive number, but was {0}", val));

      return val;
    }

    public static long ToMessageNumber(ByteString text)
    {
      var val = ToNumber(text);

      if (val == 0L)
        throw new PopMalformedTextException(string.Format("must be non-zero positive number, but was {0}", val));

      return val;
    }

    public static long ToNumber(string number)
    {
      try {
        var val = long.Parse(number);

        if (val < 0L)
          throw new PopMalformedTextException(string.Format("number must be zero or positive number, but was {0}", val));

        return val;
      }
      catch (FormatException) {
        throw new PopMalformedTextException(number);
      }
    }

    public static long ToNumber(ByteString text)
    {
      try {
        return (long)text.ToUInt64();
      }
      catch (FormatException) {
        throw new PopMalformedTextException(text);
      }
    }

    public static string ToString(ByteString text)
    {
      return text.ToString();
    }

    /*
     * utility methods
     */
    private static void ThrowIfTooFewTexts(ByteString[] texts, int expectedCount)
    {
      if (texts.Length < expectedCount)
        throw new PopMalformedTextException(string.Format("too few text counts; expected is {0} but was {1}", expectedCount, texts.Length));
    }
  }
}
