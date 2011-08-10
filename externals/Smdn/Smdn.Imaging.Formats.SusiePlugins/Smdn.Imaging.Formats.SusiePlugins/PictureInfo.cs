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

//#define GETPICTUREINFO
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Smdn.Imaging.Formats.SusiePlugins {
#if GETPICTUREINFO
  public sealed class PictureInfo : IDisposable {
    public Point Offset {
      get;
    }

    public int Width {
      get { return Size.Width; }
    }

    public int Height {
      get { return Size.Height; }
    }

    public Size Size {
      get;
    }

    public Point Density {
      get;
    }

    public ColorDepth ColorDepth {
      get;
    }

    public byte[] TextInfo {
      get;
    }

    [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PictureInfo {
      public int left;
      public int top;
      public int width;
      public int height;
      public /*WORD*/ ushort x_density;
      public /*WORD*/ushort y_density;
      public short colorDepth;
      public /*HGLOBAL*/ IntPtr hInfo;
    }

    public void Dispose()
    {
    }
  }
#endif
}
