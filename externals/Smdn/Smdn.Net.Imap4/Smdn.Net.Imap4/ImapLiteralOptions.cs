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
  [Flags]
  public enum ImapLiteralOptions : int {
    Default = NonSynchronizingIfCapable | Literal,

    /// <summary>the mask value for synchronization mode</summary>
    SynchronizationMode         = (0x10 - 1) << 0,
    /// <summary>use synchronizing literal</summary>
    Synchronizing               = 0x0 << 0,
    /// <summary>use non-synchronizing literal</summary>
    NonSynchronizing            = 0x1 << 0,
    /// <summary>use non-synchronizing literal if a server advertises the LITERAL+ capability.</summary>
    NonSynchronizingIfCapable   = 0x2 << 0,

    /// <summary>the mask value for using literal syntax</summary>
    LiteralMode                 = (0x10 - 1) << 4,
    /// <summary>use literal syntax</summary>
    Literal                     = 0x0 << 4,
    /// <summary>use literal8 syntax</summary>
    Literal8                    = 0x1 << 4,
    /// <summary>use literal8 syntax if a server advertises the BINARY capability.</summary>
    Literal8IfCapable           = 0x2 << 4,
  }
}
