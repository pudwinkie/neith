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

namespace Smdn.Formats.Feeds {
  [Flags()]
  public enum FeedVersion : int
  {
    Unknown = 0x0,

    TypeMask = unchecked((int)0xffff0000),
    Rss                       = 0x10000, // RSS 0.9x, RSS 2.0
    RdfRss                    = 0x20000, // RSS 1.0
    Atom                      = 0x40000, // Atom 0.3, Atom 1.0(RFC 4287)

    VersionMask = unchecked((int)0x0000ffff),

    Rss091 = Rss | 0x0001,
    Rss092 = Rss | 0x0002,
    Rss093 = Rss | 0x0003,
    Rss094 = Rss | 0x0004,
    Rss20  = Rss | 0x0005,

    Rss10  = RdfRss | 0x001,

    Atom03 = Atom | 0x0001,
    Atom10 = Atom | 0x0002,
  }
}