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
using System.Drawing;

using Smdn.Imaging;
using Smdn.Mathematics;

namespace Smdn.Windows.UserInterfaces {
  public class FileWallpaper : Wallpaper {
    public virtual string File {
      get; set;
    }

    public FileWallpaper()
      : this(null)
    {
    }

    public FileWallpaper(string file)
      : base()
    {
      this.File = file;
    }

    public FileWallpaper(string file, Color backgroundColor, ImageFillStyle drawStyle)
      : this(file, backgroundColor, backgroundColor, Radian.Zero, drawStyle)
    {
    }

    public FileWallpaper(string file, Color backgroundColorNear, Color backgroundColorFar, Radian gradientDirection, ImageFillStyle drawStyle)
      : base(backgroundColorNear, backgroundColorFar, gradientDirection, drawStyle)
    {
      this.File = file;
    }

    protected override void CallbackRender(Action<Bitmap> render)
    {
      if (System.IO.File.Exists(File)) {
        using (var bitmap = BitmapExtensions.LoadFrom(File)) {
          render(bitmap);
        }
      }
      else {
        render(null);
      }
    }
  }
}
