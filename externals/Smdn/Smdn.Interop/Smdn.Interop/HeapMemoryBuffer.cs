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

namespace Smdn.Interop {
  public class HeapMemoryBuffer : UnmanagedMemoryBuffer {
    private const uint HEAP_ZERO_MEMORY = 0x00000008;

    private static IntPtr hHeap = kernel32.GetProcessHeap();

    /// <value>same as return value of GetProcessHeap</value>
    public static IntPtr ProcessHeapHandle {
      get { return hHeap; }
    }

    private static IntPtr HeapAlloc(int cb)
    {
      return kernel32.HeapAlloc(hHeap, HEAP_ZERO_MEMORY, (uint)cb);
    }

    private static IntPtr HeapReAlloc(IntPtr ptr, int cb)
    {
      return kernel32.HeapReAlloc(hHeap, HEAP_ZERO_MEMORY, ptr, (uint)cb);
    }

    private static void HeapFree(IntPtr ptr)
    {
      kernel32.HeapFree(hHeap, 0, ptr);
    }

    /*
    public uint Size {
      get { return kernel32.HeapSize(hHeap, 0, base.Buffer); }
    }
    */

    public HeapMemoryBuffer(int cb)
      : base(cb, GetAllocProc(), HeapReAlloc, HeapFree)
    {
    }

    public HeapMemoryBuffer(byte[] data)
      : base(data, GetAllocProc(), HeapFree)
    {
    }

    public HeapMemoryBuffer(char[] data)
      : base(data, GetAllocProc(), HeapFree)
    {
    }

    public HeapMemoryBuffer(short[] data)
      : base(data, GetAllocProc(), HeapFree)
    {
    }

    public HeapMemoryBuffer(int[] data)
      : base(data, GetAllocProc(), HeapFree)
    {
    }

    public HeapMemoryBuffer(long[] data)
      : base(data, GetAllocProc(), HeapFree)
    {
    }

    private static AllocProc GetAllocProc()
    {
      if (Environment.OSVersion.Platform == PlatformID.Win32NT || Environment.OSVersion.Platform == PlatformID.Win32Windows)
        return HeapAlloc;
      else
        throw new PlatformNotSupportedException("supported only on Windows NT/Windows 95 or over");
    }
  }
}
