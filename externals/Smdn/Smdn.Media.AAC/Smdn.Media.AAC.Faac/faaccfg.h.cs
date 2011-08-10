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
  public enum MpegVersion : uint {
    Mpeg2 = 1,
    Mpeg4 = 0,
  }

  [CLSCompliant(false)]
  public enum AacObjectType : uint {
    Main = 1,
    Low = 2,
    SSR = 3,
    LTP = 4,
  }

  [CLSCompliant(false)]
  public enum FaacInput : uint {
    Null = 0,
    Bits16 = 1,
    Bits24 = 2,
    Bits32 = 3,
    Float  = 4,
  }

  [CLSCompliant(false)]
  public enum StreamFormat : uint {
    Raw = 0,
    ADTS = 1,
  }

  [CLSCompliant(false), StructLayout(LayoutKind.Sequential, Pack = 1)]
  public unsafe struct FaacEncConfiguration {
    public int version;
    public /* char* */ IntPtr name;
    public /* char* */ IntPtr copyright;
    public MpegVersion mpegVersion;
    public AacObjectType aacObjectType;
    public uint allowMidside;
    public uint useLfe;
    public uint useTns;
    public uint bitRate;
    public uint bandWidth;
    public uint quantqual;
    public StreamFormat outputFormat;
    public /* psymodellist_t* */ IntPtr psymodellist;
    public uint psymodelidx;
    public FaacInput inputFormat;
    public int shortctl;
    public fixed int channel_map[64];
  }
}
