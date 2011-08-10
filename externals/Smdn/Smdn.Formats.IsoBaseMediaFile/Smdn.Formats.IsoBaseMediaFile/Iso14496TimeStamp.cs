// 
// Author:
//       smdn <smdn@smdn.jp>
// 
// Copyright (c) 2009-2011 smdn
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
  public static class Iso14496TimeStamp {
    public readonly static DateTime Epoch = new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static ulong Now {
      get { return ToUInt64(DateTime.UtcNow); }
    }

    public static DateTime ToDateTime(ulong isoDateTime)
    {
      // this might overflow
      return Epoch.AddSeconds(isoDateTime);
    }

    public static DateTime ToDateTime(uint isoDateTime)
    {
      return Epoch.AddSeconds(isoDateTime);
    }

    public static ulong ToUInt64(DateTimeOffset dateTimeOffset)
    {
      return ToUInt64(dateTimeOffset.UtcDateTime);
    }

    public static ulong ToUInt64(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc)
        dateTime = dateTime.ToUniversalTime();

      return (ulong)dateTime.Subtract(Epoch).TotalSeconds;
    }

    public static uint ToUInt32(DateTimeOffset dateTimeOffset)
    {
      return ToUInt32(dateTimeOffset.UtcDateTime);
    }

    public static uint ToUInt32(DateTime dateTime)
    {
      if (dateTime.Kind != DateTimeKind.Utc)
        dateTime = dateTime.ToUniversalTime();

      return (uint)dateTime.Subtract(Epoch).TotalSeconds;
    }
  }
}
