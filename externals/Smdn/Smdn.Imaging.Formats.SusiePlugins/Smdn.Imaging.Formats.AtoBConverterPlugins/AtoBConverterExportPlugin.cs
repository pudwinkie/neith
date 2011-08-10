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
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

using Smdn.Interop;
using Smdn.Imaging.Formats.SusiePlugins;

namespace Smdn.Imaging.Formats.AtoBConverterPlugins {
  public class AtoBConverterExportPlugin : SusiePlugin, IImageEncoder {
    [UnmanagedFunctionPointer(CallingConvention.StdCall)] public delegate bool XpiIsSupported(int colorDepth);

    internal AtoBConverterExportPlugin(DynamicLinkLibrary library, SpiGetPluginInfo getPluginInfo, SusiePluginApiVersion apiVersion)
      : base(library, getPluginInfo, apiVersion)
    {
      isSupported = Library.GetFunction<XpiIsSupported>("IsSupported");
    }

    public bool IsSupported(ColorDepth colorDepth)
    {
      return IsSupported((int)colorDepth);
    }

    public bool IsSupported(int colorDepth)
    {
      CheckDisposed();

      return isSupported(colorDepth);
    }

#region "IImageEncoder implementation"
    void IImageEncoder.Encode(Bitmap bitmap, Stream stream, EncoderParameters encoderParams)
    {
      throw new NotImplementedException();
    }
#endregion

    private XpiIsSupported isSupported;
  }
}
