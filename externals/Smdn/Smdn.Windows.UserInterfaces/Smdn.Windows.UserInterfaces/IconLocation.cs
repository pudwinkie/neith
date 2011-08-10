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
#if !DISABLE_SMDN_IMAGING_FORMATS_ICO
using System.Drawing;
#endif

namespace Smdn.Windows.UserInterfaces {
  public struct IconLocation {
    public static readonly IconLocation Empty = new IconLocation();

    public string File {
      get { return file; }
      set { file = value; }
    }

    public int Index {
      get { return index; }
      set { index = CheckIndex(value); }
    }

    public IconLocation(string file, int index)
      : this()
    {
      this.file = file;
      this.index = CheckIndex(index);
    }

#if !DISABLE_SMDN_IMAGING_FORMATS_ICO
    public Bitmap Extract()
    {
      if (File == null)
        return null;
      else
        return Smdn.Imaging.Formats.Ico.Icon.Extract(File, index);
    }
#endif

    private int CheckIndex(int val)
    {
      if (val < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("val", val);

      return val;
    }

    private string file;
    private int index;
  }
}