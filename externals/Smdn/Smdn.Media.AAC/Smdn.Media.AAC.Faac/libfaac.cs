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

namespace Smdn.Media.AAC.Faac {
  [CLSCompliant(false)]
  public static class libfaac {
    private const string dllname = "libfaac";

    [DllImport(dllname)] public static extern int faacEncGetVersion(out IntPtr faac_id_string, out IntPtr faac_copyright_string);
    [DllImport(dllname)] public static extern IntPtr faacEncOpen(uint sampleRate, uint numChannels, out uint inputSamples, out uint maxOutputBytes);
    [DllImport(dllname)] public static extern int faacEncClose(IntPtr hEncoder);
    [DllImport(dllname)] public static extern unsafe FaacEncConfiguration* faacEncGetCurrentConfiguration(IntPtr hEncoder);
    [DllImport(dllname)] public static extern unsafe int faacEncSetConfiguration(IntPtr hEncoder, FaacEncConfiguration* config);
    [DllImport(dllname)] public static extern int faacEncGetDecoderSpecificInfo(IntPtr hEncoder, out IntPtr ppBuffer, out uint pSizeOfDecoderSpecificInfo);
    [DllImport(dllname)] public static extern unsafe int faacEncEncode(IntPtr hEncoder, int* inputBuffer, uint samplesInput, byte* outputBuffer, uint bufferSize);

    private static int version = 0;
    private static string copyright = null;
    private static string id = null;

    public static bool IsAvailable {
      get { return Version != 0; }
    }

    public static int Version {
      get { TryGetVersion(); return version; }
    }

    public static string Copyright {
      get { TryGetVersion(); return copyright; }
    }

    public static string Id {
      get { TryGetVersion(); return id; }
    }

    private static void TryGetVersion()
    {
      if (version != 0)
        return;

      try {
        IntPtr idStringPtr, copyrightStringPtr;

        version = faacEncGetVersion(out idStringPtr, out copyrightStringPtr);

        id = Marshal.PtrToStringAnsi(idStringPtr);
        copyright = Marshal.PtrToStringAnsi(copyrightStringPtr);

        // DO NOT free ptr
      }
      catch {
      }
    }
  }
}
