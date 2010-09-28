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

namespace Smdn.Net.Imap4 {
  public sealed class ImapStatusAttributeList {
    public long? Messages { get; set; }
    public long? Recent { get; set; }
    public long? Unseen { get; set; }
    public long? UidNext { get; set; }
    public long? UidValidity { get; set; }
    [CLSCompliant(false)] public ulong? HighestModSeq { get; set; }

    public ImapStatusAttributeList()
    {
      this.Messages = null;
      this.Recent = null;
      this.Unseen = null;
      this.UidNext = null;
      this.UidValidity = null;
      this.HighestModSeq = null;
    }

    public override string ToString()
    {
      return string.Format("{{Messages={0}, Recent={1}, Unseen={2}, UidNext={3}, UidValidity={4}, HighestModSeq={5}}}", Messages, Recent, Unseen, UidNext, UidValidity, HighestModSeq);
    }
  }
}