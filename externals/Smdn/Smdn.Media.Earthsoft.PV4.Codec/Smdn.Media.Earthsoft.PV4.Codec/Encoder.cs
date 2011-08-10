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
using System.Drawing;
using System.Threading;

using Smdn.Formats.Earthsoft.PV4;
using Smdn.Interop;
using Smdn.Mathematics;

namespace Smdn.Media.Earthsoft.PV4.Codec {
  public abstract class Encoder : CodecBase {
    public static Encoder Create(DV dv)
    {
      return Create(dv, CodecBase.DefaultThreadCount, null);
    }

    public static Encoder Create(DV dv, int threadCount)
    {
      return Create(dv, threadCount, null);
    }

    public static Encoder Create(DV dv, Type forceDctType)
    {
      return Create(dv, CodecBase.DefaultThreadCount, forceDctType);
    }

    public static Encoder Create(DV dv, int threadCount, Type forceDctType)
    {
      throw new NotImplementedException();
#if false
      return new Simple.Encoder(dv, threadCount, forceDctType);
#endif
    }

    protected Encoder(DV dv, int threadCount)
      : base(dv, threadCount)
    {
    }

    protected abstract StreamFileFrameData EncodePackedYUV422Frame(IntPtr packedYuv422, Fraction displayAspectRatio);

    public StreamFileFrameData EncodeFrame(Bitmap bitmap, Fraction displayAspectRatio)
    {
      throw new NotImplementedException();
    }
  }
}
