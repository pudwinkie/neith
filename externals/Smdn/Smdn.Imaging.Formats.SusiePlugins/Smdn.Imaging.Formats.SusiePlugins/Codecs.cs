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
using System.Collections.Generic;

#if NET_3_5
using System.Linq;
#else
using Smdn.Collections;
#endif

namespace Smdn.Imaging.Formats.SusiePlugins {
  public class Codecs : IImageCodecs {
    private IEnumerable<SusiePlugin> GetLoadedPlugins()
    {
      var plugins = SusiePlugin.GetLoadedPlugins();

      if (plugins.Count() == 0)
        return SusiePlugin.LoadInstalled();
      else
        return plugins;
    }

    public IEnumerable<IImageDecoder> CreateDecoders()
    {
      foreach (var plugin in GetLoadedPlugins()) {
        if (plugin is IImageDecoder)
          yield return plugin as IImageDecoder;
      }
    }

    public IEnumerable<IImageEncoder> CreateEncoders()
    {
      foreach (var plugin in GetLoadedPlugins()) {
        if (plugin is IImageEncoder)
          yield return plugin as IImageEncoder;
      }
    }
  }
}
