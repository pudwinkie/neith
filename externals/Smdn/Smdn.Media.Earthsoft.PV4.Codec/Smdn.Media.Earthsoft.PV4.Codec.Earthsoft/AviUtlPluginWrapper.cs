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
using System.Runtime.InteropServices;

using Smdn.Interop;

namespace Smdn.Media.Earthsoft.PV4.Codec.Earthsoft {
  internal class AviUtlPluginWrapper : IDisposable {
    /*
    unsigned int  size;

    BOOL      (*func_init  )(void);
    void      (*func_exit  )(void);

    INPUT_HANDLE  (*func_open  )(void);
    void      (*func_close )(INPUT_HANDLE);

    void      (*func_header)(INPUT_HANDLE, const void *header_ptr);
    void      (*func_decode)(INPUT_HANDLE, const void *dv_ptr, void *pixel_ptr, int pixel_stride);
    */

    private delegate IntPtr GetInputPluginTable2();
    private delegate bool PluginFuncInit();
    private delegate void PluginFuncExit();
    private delegate IntPtr PluginFuncOpen();
    private delegate void PluginFuncClose(IntPtr inputHandle);
    private delegate void PluginFuncHeader(IntPtr inputHandle, IntPtr headerPtr);
    private delegate void PluginFuncDecode(IntPtr inputHandle, IntPtr dvPtr, IntPtr pixelPtr, int pixelStride);

    public AviUtlPluginWrapper(string auiPath)
    {
      module = new DynamicLinkLibrary(auiPath);

      InitializeFunctionPointers();
    }

    private void InitializeFunctionPointers()
    {
      var getInputPluginTable = module.GetFunction<GetInputPluginTable2>("GetInputPluginTable2");

      var funcTable = getInputPluginTable();

      if (funcTable == IntPtr.Zero)
        throw new NotSupportedException("GetInputPluginTable2 failed");

      func_init   = GetDelegateFromFunctionTable<PluginFuncInit  >(funcTable, IntPtr.Size * 1);
      func_exit   = GetDelegateFromFunctionTable<PluginFuncExit  >(funcTable, IntPtr.Size * 2);
      func_open   = GetDelegateFromFunctionTable<PluginFuncOpen  >(funcTable, IntPtr.Size * 3);
      func_close  = GetDelegateFromFunctionTable<PluginFuncClose >(funcTable, IntPtr.Size * 4);
      func_header = GetDelegateFromFunctionTable<PluginFuncHeader>(funcTable, IntPtr.Size * 5);
      func_decode = GetDelegateFromFunctionTable<PluginFuncDecode>(funcTable, IntPtr.Size * 6);
    }

    private TDelegate GetDelegateFromFunctionTable<TDelegate>(IntPtr functionTable, int offset) where TDelegate : class
    {
      return DynamicLibrary.GetDelegateForFunctionPointer<TDelegate>(Marshal.ReadIntPtr(functionTable, offset));
    }

    public bool Init()
    {
      RejectDisposed();

      if (func_init == null)
        throw new FunctionNotFoundException("func_init", module.Path);

      return func_init();
    }

    public void Exit()
    {
      RejectDisposed();

      if (func_exit == null)
        throw new FunctionNotFoundException("func_exit", module.Path);

      func_exit();
    }

    public IntPtr Open()
    {
      RejectDisposed();

      if (func_open == null)
        throw new FunctionNotFoundException("func_open", module.Path);

      return func_open();
    }

    public void Close(IntPtr inputHandle)
    {
      RejectDisposed();

      if (func_close == null)
        throw new FunctionNotFoundException("func_close", module.Path);

      func_close(inputHandle);
    }

    public void Header(IntPtr inputHandle, IntPtr headerPtr)
    {
      RejectDisposed();

      if (func_header == null)
        throw new FunctionNotFoundException("func_header", module.Path);

      func_header(inputHandle, headerPtr);
    }

    public void Decode(IntPtr inputHandle, IntPtr dvPtr, IntPtr pixelPtr, int pixelStride)
    {
      RejectDisposed();

      if (func_decode == null)
        throw new FunctionNotFoundException("func_decode", module.Path);

      func_decode(inputHandle, dvPtr, pixelPtr, pixelStride);
    }

    private void RejectDisposed()
    {
      if (module == null)
        throw new ObjectDisposedException(GetType().Name);
    }

    public void Dispose()
    {
      if (module != null) {
        module.Free();
        module = null;
      }

      func_init = null;
      func_exit = null;
      func_open = null;
      func_close = null;
      func_header = null;
      func_decode = null;
    }

    private DynamicLinkLibrary module;
    private PluginFuncInit func_init = null;
    private PluginFuncExit func_exit = null;
    private PluginFuncOpen func_open = null;
    private PluginFuncClose func_close = null;
    private PluginFuncHeader func_header = null;
    private PluginFuncDecode func_decode = null;
  }
}
