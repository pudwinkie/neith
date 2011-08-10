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

namespace Smdn.Imaging.Formats.SusiePlugins {
  [Flags]
  public enum SusiePluginApiVersion : int
  {
    ArchiveTypeMask   = 0xff << 0,
    Normal            = ((byte)'N') << 0,
    MultiPicture      = ((byte)'M') << 0,

    FilterTypeMask    = 0xff << 8,
    ImportFilter      = ((byte)'I') << 8,
    ExportFilter      = ((byte)'X') << 8,
    ArchiveExtractor  = ((byte)'A') << 8,

    VersionMask       = 0xffff << 16,
    Version00         = ((byte)'0') << 24 | ((byte)'0') << 16,
    VersionT0         = ((byte)'T') << 24 | ((byte)'0') << 16,
  }
}
