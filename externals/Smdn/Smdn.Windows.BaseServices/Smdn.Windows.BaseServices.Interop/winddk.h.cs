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

namespace Smdn.Windows.BaseServices.Interop {
  public static partial class Consts {
    public const uint FILE_LIST_DIRECTORY         = 0x00000001;
    public const uint FILE_READ_DATA              = 0x00000001;
    public const uint FILE_ADD_FILE               = 0x00000002;
    public const uint FILE_WRITE_DATA             = 0x00000002;
    public const uint FILE_ADD_SUBDIRECTORY       = 0x00000004;
    public const uint FILE_APPEND_DATA            = 0x00000004;
    public const uint FILE_CREATE_PIPE_INSTANCE   = 0x00000004;
    public const uint FILE_READ_EA                = 0x00000008;
    public const uint FILE_READ_PROPERTIES        = 0x00000008;
    public const uint FILE_WRITE_EA               = 0x00000010;
    public const uint FILE_WRITE_PROPERTIES       = 0x00000010;
    public const uint FILE_EXECUTE                = 0x00000020;
    public const uint FILE_TRAVERSE               = 0x00000020;
    public const uint FILE_DELETE_CHILD           = 0x00000040;
    public const uint FILE_READ_ATTRIBUTES        = 0x00000080;
    public const uint FILE_WRITE_ATTRIBUTES       = 0x00000100;
  }
}