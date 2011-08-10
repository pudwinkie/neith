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
using System.Runtime.InteropServices;

using Smdn.Imaging.Interop;

namespace Smdn.Windows.UserInterfaces.Interop {
  [StructLayout(LayoutKind.Sequential, Pack = 4)]
  public struct MARGINS {
    public int cxLeftWidth;
    public int cxRightWidth;
    public int cyTopHeight;
    public int cyBottomHeight;

    public MARGINS(int leftWidth, int rightWidth, int topHeight, int bottomHeight)
    {
      this.cxLeftWidth    = leftWidth;
      this.cxRightWidth   = rightWidth;
      this.cyTopHeight    = topHeight;
      this.cyBottomHeight = bottomHeight;
    }

    /// <remarks>Negative margins are used to create the "sheet of glass" effect where the client area is rendered as a solid surface with no window border.</remarks>
    public static readonly MARGINS ClientAll = new MARGINS(-1, 0, 0, 0);
  }
}