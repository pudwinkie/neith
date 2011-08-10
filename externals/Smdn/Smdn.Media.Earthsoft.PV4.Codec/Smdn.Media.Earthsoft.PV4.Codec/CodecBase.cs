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
using System.Threading;

using Smdn.Formats.Earthsoft.PV4;

namespace Smdn.Media.Earthsoft.PV4.Codec {
  public abstract class CodecBase : IDisposable {
    protected static int DefaultThreadCount {
      get { return (1 < Environment.ProcessorCount ? Environment.ProcessorCount : 0); }
    }

    public DV DV {
      get { return dv; }
    }

    protected int ThreadCount {
      get { return threadCount; }
    }

    protected AutoResetEvent[] ThreadWaitHandles {
      get { return threadWaitHandles; }
    }

    protected CodecBase(DV dv, int threadCount)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");

      if (threadCount < 0)
        throw ExceptionUtils.CreateArgumentMustBeZeroOrPositive("threadCount", threadCount);

      this.dv = dv;
      this.threadCount = threadCount;

      threadWaitHandles = new AutoResetEvent[threadCount];

      for (var i = 0; i < threadCount; i++) {
        threadWaitHandles[i] = new AutoResetEvent(false);
      }
    }

    ~CodecBase()
    {
      Dispose(false);
    }

    void IDisposable.Dispose()
    {
      Close();
    }

    public void Close()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing) {
        if (threadWaitHandles != null) {
          foreach (var threadWaitHandle in threadWaitHandles) {
            threadWaitHandle.Close();
          }
          threadWaitHandles = null;
        }
      }

      disposed = true;
    }

    protected void CheckDisposed()
    {
      if (disposed)
        throw new ObjectDisposedException(GetType().Name);
    }

    private DV dv;
    private readonly int threadCount;
    private AutoResetEvent[] threadWaitHandles;
    private bool disposed = false;
  }
}
